using System;
using System.IO;
using MonoPlus.Modding;
using Serilog;

namespace DerelictDimension;

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
        if (args.Length < 3) throw new ArgumentException("Command \"mods\" requires second argument!");
        switch (args[2])
        {
            case "create":
                Create(args);
                break;
            case "validate":
                Validate(args);
                break;
            default:
                throw new ArgumentException($"Unknown command: {args[2]}");
        }
    }

    public static void Create(string[] args)
    {
        Console.Write("Enter new mod name: ");
        string? modName = null;
        while (modName is null) modName = Console.ReadLine();
        Console.WriteLine($"ModName: {modName}");
        Console.ReadLine();
    }

    public static void Validate(string[] args)
    {
        if (args.Length < 4 || !Path.Exists($"{ModManager.ModsDirectory}{args[3]}/"))
            throw new ArgumentException("Invalid modname to validate (not specified/folder doesn't exist)");
        Exception? validationResult = ModValidator.ValidateMod(args[3]);
        if (validationResult is not null) throw validationResult;
    }
}
