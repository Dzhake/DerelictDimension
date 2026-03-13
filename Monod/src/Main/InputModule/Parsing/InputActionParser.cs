using Monod.InputModule.InputActions;

namespace Monod.InputModule.Parsing;

/// <summary>
/// Fast span-based parser for <see cref="InputAction"/> textual representations.
/// </summary>
public static class InputActionParser
{
    /// <summary>
    /// List of all errors for the last <see cref="Parse"/>.
    /// </summary>
    public readonly static List<ActionParseError> Errors = new();

    /// <summary>
    /// Parse <see cref="InputAction"/> from the <paramref name="text"/>.
    /// </summary>
    /// <param name="text">Text to parse.</param>
    /// <returns>Parsed <see cref="InputAction"/> or <see cref="InvalidInputAction"/> on error.</returns>
    public static InputAction Parse(string text)
    {
        Errors.Clear();

        SpanParser parser = new SpanParser(text);
        InputAction result = ParseExpression(ref parser, text);
        parser.SkipWhitespaces();
        if (parser.CanRead() && result is not InvalidInputAction)
        {
            int start = parser.position;
            int length = parser.source.Length - start;
            return Invalid("Unexpected characters after expression", start, length, text);
        }

        return result;
    }

    private static InputAction ParseExpression(ref SpanParser parser, string originalText)
    {
        int exprStart = parser.position;
        parser.SkipWhitespaces();
        if (!parser.CanRead())
            return Invalid("Unexpected end of input", parser.position, 0, originalText);

        int nameStart = parser.position;
        ReadOnlySpan<char> nameSpan = ReadWhile(ref parser, static c => char.IsAsciiLetter(c));

        if (nameSpan.IsEmpty)
        {
            int errorStart = parser.position;
            SkipToNextSeparator(ref parser);
            return Invalid("Expected action name", errorStart, 1, originalText);
        }

        int nameLength = parser.position - nameStart;
        ReadOnlySpan<char> actionName = parser.source.Slice(nameStart, nameLength);

        if (!IsKnownActionName(actionName))
        {
            int errorStart = nameStart;
            int errorEnd = parser.position;
            SkipToNextSeparator(ref parser);
            return Invalid("Unknown action name", errorStart, errorEnd - errorStart, originalText);
        }

        parser.SkipWhitespaces();
        if (!parser.Skip('('))
        {
            int errorStart = nameStart;
            SkipToNextSeparator(ref parser);
            int errorEnd = parser.position;
            return Invalid($"Expected '(' after {actionName}", errorStart, errorEnd - errorStart, originalText);
        }

        parser.SkipWhitespaces();
        return ParseActionArguments(ref parser, originalText, nameStart, nameLength);
    }

    private static bool IsKnownActionName(ReadOnlySpan<char> name)
    {
        return name switch
        {
            "Or" or "And" or "Down" or "Up" or "Pressed" or "Released" or "Held" => true,
            _ => false
        };
    }

    private static void SkipToNextSeparator(ref SpanParser parser)
    {
        int depth = 0;
        while (parser.CanRead())
        {
            char c = parser.Peek();
            if (c == '(')
                depth++;
            else if (c == ')')
            {
                if (depth == 0)
                    break;
                depth--;
            }
            else if (c == ',' && depth == 0)
                break;

            parser.Read();
        }
    }

    private static InputAction ParseActionArguments(ref SpanParser parser, string originalText,
        int nameStart, int nameLength)
    {
        ReadOnlySpan<char> actionName = parser.source.Slice(nameStart, nameLength);

        if (actionName.SequenceEqual("Or".AsSpan()) || actionName.SequenceEqual("And".AsSpan()))
        {
            parser.SkipWhitespaces();
            if (parser.Peek() == ')')
            {
                parser.Skip(')');
                return actionName.SequenceEqual("Or".AsSpan())
                    ? new OrAction(Array.Empty<InputAction>())
                    : new AndAction(Array.Empty<InputAction>());
            }

            var actions = new List<InputAction>();
            while (true)
            {
                parser.SkipWhitespaces();
                InputAction arg = ParseExpression(ref parser, originalText);
                actions.Add(arg);

                parser.SkipWhitespaces();
                if (parser.Skip(','))
                {
                    parser.SkipWhitespaces();
                }
                else if (parser.Skip(')'))
                {
                    break;
                }
                else
                {
                    int errStart = parser.position;
                    SkipToNextSeparator(ref parser);
                    int errEnd = parser.position;
                    actions.Add(Invalid("Expected ',' or ')'", errStart, 1, originalText));

                    parser.SkipWhitespaces();
                    if (parser.Skip(','))
                    {
                        parser.SkipWhitespaces();
                    }
                    else if (parser.Skip(')'))
                    {
                        break;
                    }
                    else
                    {
                        return actions.Last();
                    }
                }
            }

            return actionName.SequenceEqual("Or".AsSpan())
                ? new OrAction(actions.ToArray())
                : new AndAction(actions.ToArray());
        }
        else
        {
            parser.SkipWhitespaces();
            int keyStart = parser.position;
            ReadOnlySpan<char> keySpan = ReadWhile(ref parser, static c => char.IsAsciiLetterOrDigit(c));
            if (keySpan.IsEmpty)
            {
                int errorStart = nameStart;
                SkipToNextSeparator(ref parser);
                int errorEnd = parser.position;
                return Invalid("Expected key name", errorStart, errorEnd - errorStart, originalText);
            }

            parser.SkipWhitespaces();
            if (!parser.Skip(')'))
            {
                int errorStart = nameStart;
                SkipToNextSeparator(ref parser);
                int errorEnd = parser.position;
                return Invalid("Expected ')' after key", errorStart, errorEnd - errorStart, originalText);
            }

            if (!Enum.TryParse(keySpan, out Key key))
            {
                int errorStart = keyStart;
                SkipToNextSeparator(ref parser);
                int errorEnd = parser.position - 1;
                return Invalid("Invalid key name", errorStart, errorEnd - errorStart, originalText);
            }

            if (actionName.SequenceEqual("Down".AsSpan())) return new DownAction(key);
            if (actionName.SequenceEqual("Up".AsSpan())) return new UpAction(key);
            if (actionName.SequenceEqual("Pressed".AsSpan())) return new PressedAction(key);
            if (actionName.SequenceEqual("Released".AsSpan())) return new ReleasedAction(key);
            if (actionName.SequenceEqual("Held".AsSpan())) return new HeldAction(key);

            int unknownStart = nameStart;
            SkipToNextSeparator(ref parser);
            int unknownEnd = keyStart - 1;
            return Invalid("Unknown action name", unknownStart, unknownEnd - unknownStart, originalText);
        }
    }

    private static ReadOnlySpan<char> ReadWhile(ref SpanParser parser, Func<char, bool> predicate)
    {
        int start = parser.position;
        while (parser.CanRead() && predicate(parser.Peek()))
            parser.Read();
        return parser.source.Slice(start, parser.position - start);
    }

    /// <summary>
    /// Records an error and returns an <see cref="InvalidInputAction"/> that wraps the original input text.
    /// We deliberately return the original <paramref name="originalText"/> so the InvalidInputAction contains
    /// the full input (allocation unavoidable: the caller provided a string). Other allocations are minimized
    /// (spans used for parsing; the only allocations are the errors list entries and arrays for final actions).
    /// </summary>
    private static InvalidInputAction Invalid(string message, int startIndex, int length, string originalText)
    {
        Errors.Add(new ActionParseError(message, startIndex, length));
        return new InvalidInputAction(originalText);
    }
}
