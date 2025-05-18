using System;
using CommandLine;
using CommandLine.Text;
using Serilog;

namespace DerelictDimension.ModsTool;

/// <summary>
/// Wrapper around <see cref="ModBuilder"/> for <see cref="Console"/>
/// </summary>
public static class ModsCLI
{
    /// <summary>
    /// Runs <see cref="ModsCLI"/> with specified <paramref name="args"/>
    /// </summary>
    /// <param name="args">Command-line arguments, where first argument is path to .exe, second is anything, third is a valid <see cref="ModsCLI"/> command, and the other are anything.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="args"/> are invalid, or some are missing</exception>
    public static void Run(string[] args)
    {
        Log.Information("Running ModsCLI.");
        var parser = new Parser(with => with.HelpWriter = null);
        var parserResult = parser.ParseArguments(args, typeof(CreateMod), typeof(ValidateMod));
        parserResult
        .WithParsed(RunOptions)
        .WithNotParsed(_ =>
        {
            var helpText = HelpText.AutoBuild(parserResult, h =>
            {
                h.AutoHelp = false;     // hides --help
                h.AutoVersion = false;  // hides --version
                return HelpText.DefaultParsingErrorsHandler(parserResult, h);
            }, e => e);
            Console.WriteLine(helpText);
        });
        AwaitInput();
    }

    /// <summary>
    /// Runs <paramref name="options"/> if it's <see cref="IRunnableOptions"/>, does nothing otherwise
    /// </summary>
    /// <param name="options"><see cref="IRunnableOptions"/> to run, or anything else to do nothing</param>
    public static void RunOptions(object options)
    {
        if (options is not IRunnableOptions runnableOptions) return;

        int result = runnableOptions.Run();
        Console.WriteLine($"Command exited with code {result}");
        AwaitInput();
        Environment.Exit(result);
    }

    private static void AwaitInput()
    {
        
        Console.WriteLine("Press any key to continue..");
        Console.ReadKey();
    }
}
