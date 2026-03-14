using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Monod.InputModule;

public sealed class KeybindJsonConverter : JsonConverter<Keybind>
{
    public override Keybind Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("Expected StartObject when reading Keybind.");
        }

        Key key = Key.None;
        KeyModifiers modifiers = KeyModifiers.None;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                break;
            }

            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException("Expected PropertyName when reading Keybind.");
            }

            string propertyName = reader.GetString()!;
            reader.Read();

            switch (propertyName)
            {
                case "key":
                    if (reader.TokenType == JsonTokenType.String)
                    {
                        string keyName = reader.GetString()!;
                        if (!Enum.TryParse<Key>(keyName, ignoreCase: true, out key))
                        {
                            throw new JsonException($"Unknown key value '{keyName}' when parsing Keybind.");
                        }
                    }
                    else if (reader.TokenType == JsonTokenType.Number)
                    {
                        if (reader.TryGetInt32(out int intVal))
                        {
                            key = (Key)intVal;
                        }
                        else
                        {
                            throw new JsonException("Invalid numeric value for key in Keybind.");
                        }
                    }
                    else
                    {
                        throw new JsonException("Invalid token for 'key' property in Keybind.");
                    }
                    break;

                case "modifiers":
                    if (reader.TokenType == JsonTokenType.String)
                    {
                        string modStr = reader.GetString()!;
                        if (string.Equals(modStr, "Any", StringComparison.OrdinalIgnoreCase))
                        {
                            modifiers = KeyModifiers.Any;
                        }
                        else if (string.Equals(modStr, "None", StringComparison.OrdinalIgnoreCase) || string.IsNullOrWhiteSpace(modStr))
                        {
                            modifiers = KeyModifiers.None;
                        }
                        else
                        {
                            KeyModifiers combined = KeyModifiers.None;
                            string[] parts = modStr.Split('|', StringSplitOptions.RemoveEmptyEntries);
                            foreach (var part in parts)
                            {
                                string token = part.Trim();
                                if (Enum.TryParse<KeyModifiers>(token, ignoreCase: true, out var parsed))
                                {
                                    combined |= parsed;
                                }
                                else
                                {
                                    throw new JsonException($"Unknown modifier '{token}' when parsing Keybind.");
                                }
                            }

                            modifiers = combined;
                        }
                    }
                    else
                    {
                        throw new JsonException("Invalid token for 'modifiers' property in Keybind.");
                    }
                    break;

                default:
                    // Skip unknown properties
                    reader.Skip();
                    break;
            }
        }

        return new Keybind(key, modifiers);
    }

    public override void Write(Utf8JsonWriter writer, Keybind value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        writer.WriteString("key", value.key.ToString());

        var mods = value.modifiers;
        if (mods == KeyModifiers.Any)
        {
            writer.WriteString("modifiers", "Any");
        }
        else if (mods != KeyModifiers.None)
        {
            var names = new List<string>();
            if (mods.HasFlag(KeyModifiers.Ctrl))
            {
                names.Add("Ctrl");
            }
            if (mods.HasFlag(KeyModifiers.Shift))
            {
                names.Add("Shift");
            }
            if (mods.HasFlag(KeyModifiers.Alt))
            {
                names.Add("Alt");
            }

            if (names.Count > 0)
            {
                writer.WriteString("modifiers", string.Join('|', names));
            }
        }

        writer.WriteEndObject();
    }
}
