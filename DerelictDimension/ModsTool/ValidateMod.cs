using System;
using Chasm.SemanticVersioning;
using CommandLine;
using MonoPlus.Modding;

namespace DerelictDimension.ModsTool;

/// <summary>
/// Represents options for validating a mod, and <see cref="Run"/> method to run validation after options are set
/// </summary>
[Verb("validate", HelpText = "Validates that mod is correct and doesn't have any common mistakes")]
public class ValidateMod : IRunnableOptions
{
    /// <summary>
    /// Name of the mod
    /// </summary>
    [Value(0, HelpText = "Name of the mod")]
    public required string ModName { get; set; }

    /// <inheritdoc/>  
    public int Run()
    {
        //load mod from folder, get config, validate, etc.
        ModConfig config = ModLoader.LoadModConfigFromFolder(ModManager.ModsDirectory+ModName);
        ModId id = config.Id;
        string name = id.Name;
        if (id.Version == new SemanticVersion(0, 0, 0))
        {
            Console.WriteLine("Mod version is 0.0.0, you must use 0.0.1 or above");
            return 1;
        }
        
        return 0;
    }
}
