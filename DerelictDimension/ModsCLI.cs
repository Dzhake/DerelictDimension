using System;
using System.IO;
using MonoPlus.Modding;
using Serilog;

namespace DerelictDimension;

public static class ModsCLI
{
    public static void Run(string[] args)
    {
        Log.Information("Running ModsCLI.");
        if (args.Length < 3) throw new ArgumentException("Command \"mods\" requires second argument!");
        switch (args[2])
        {
            case "create":
                Create();
                break;
            case "validate":
                if (args.Length < 4 || !Path.Exists($"{ModManager.ModsDirectory}{args[3]}/"))
                    throw new Exception("Invalid modname to validate (not specified/folder doesn't exist)");
                Exception? validationResult = ModValidator.ValidateMod(args[3]);
                if (validationResult is not null) throw validationResult;
                break;
            default:
                Log.Fatal("Unknown command {Command}", args[2]);
                break;
        }
    }

    private static void Create()
    {
        Console.WriteLine(":)");
        string? modName = null;
        while (modName is null) modName = Console.ReadLine();
        Console.WriteLine(modName);
        Console.ReadLine();
    }
}
