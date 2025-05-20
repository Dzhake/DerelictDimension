using System;
using System.IO;
using System.Text.Json;
using CommandLine;
using MonoPlus.Modding;
using MonoPlus.Utils;

namespace DerelictDimension.ModsTool;

/// <summary>
/// Represents options for creating a new mod, and <see cref="Run"/> method to run creation after options are set
/// </summary>
[Verb("create", HelpText = "Create a new mod")]
public class CreateMod : IRunnableOptions
{
    /// <summary>
    /// Name of new mod
    /// </summary>
    [Value(0, HelpText = "Name of new mod", Required = true)]
    public required string ModName { get; set; }

    /// <summary>
    /// Whether new mod should include files related to code (e.g. .csproj and .cs files)
    /// </summary>
    [Option(HelpText = "Whether new mod should include files related to code (e.g. .csproj and .cs files)")]
    public bool Code { get; set; }

    /// <inheritdoc/> 
    public int Run()
    {
        string modDir = $"{ModManager.ModsDirectory}{ModName}/";
        if (Directory.Exists(modDir))
        {
            Console.WriteLine("Mod directory with same name already exists");
            return 1;
        }

        Directory.CreateDirectory(modDir);

        ModConfig config = new()
        {
            Id = new(ModName, new(1, 0, 0))
        };
        if (Code) config.AssemblyFile = $"bin/{ModName}.dll";

        FileStream configStream = new($"{modDir}config.json", FileMode.Create);
        JsonSerializer.Serialize(configStream, config, Json.Readable);
        configStream.Close();
        if (Code) WriteCode(modDir);

        return 0;
    }

    /// <summary>
    /// Creates everything inside Source directory (and the Source directory)
    /// </summary>
    /// <param name="modDir"><see cref="Directory"/> path inside which Source should be located</param>
    private void WriteCode(string modDir)
    {
        string sourceDir = $"{modDir}Source/";
        Directory.CreateDirectory(sourceDir);

        StreamWriter projectWriter = new($"{sourceDir}{ModName}.csproj");
        WriteProjectFile(projectWriter);
        projectWriter.Close();

        StreamWriter modFileWriter = new($"{sourceDir}{ModName}Core.cs");
        WriteModFile(modFileWriter);
        modFileWriter.Close();
    }

    private void WriteModFile(StreamWriter writer)
    {
        writer.WriteLine($$"""
                         using MonoPlus.Modding;
                         
                         namespace {{ModName}};
                         
                         public class {{ModName}}Core : Mod
                         {
                         
                         }
                         """);
    }

    /// <summary>
    /// Write template .csproj file with <paramref name="writer"/>
    /// </summary>
    /// <param name="writer">Writer which should write the project file</param>
    private void WriteProjectFile(StreamWriter writer)
    {
        writer.WriteLine("<Project Sdk=\"Microsoft.NET.Sdk\">");
        writer.WriteLine();

        writer.WriteLine("<PropertyGroup>");
        WriteProjectProperties(writer);
        writer.WriteLine("</PropertyGroup>");
        writer.WriteLine();

        writer.WriteLine("<ItemGroup>");
        WriteProjectItems(writer);
        writer.WriteLine("</ItemGroup>");
        writer.WriteLine();

        WriteProjectTasks(writer);
        writer.WriteLine();

        writer.WriteLine("</Project>");
    }

    /// <summary>
    /// Write properties, must be used at PropertyGroup level
    /// </summary>
    /// <param name="writer">Writer which should write properties</param>
    private void WriteProjectProperties(StreamWriter writer)
    {
        writer.WriteLine("""
                         <TargetFramework>net9.0</TargetFramework>
                         <Nullable>enable</Nullable>
                         <AppendTargetFrameworkToOutputPath>false</AppendTargetFrameworkToOutputPath>
                         <AppendRuntimeIdentifierToOutputPath>false</AppendRuntimeIdentifierToOutputPath>
                         <DebugType>embedded</DebugType>
                         """);
        //<GenerateDocumentationFile>true</GenerateDocumentationFile>
    }

    /// <summary>
    /// Write items, must be used at ItemGroup level
    /// </summary>
    /// <param name="writer">Writer which should write items</param>
    private void WriteProjectItems(StreamWriter writer)
    {
        //<Private> is actually "Copy Local"..       ???

        writer.WriteLine("""
                         <Reference Include=\"MonoPlus\">
                         <HintPath>../../../MonoPlus.dll</HintPath>
                         <Private>False</Private>
                         </Reference>
                         
                         writer.WriteLine($"<Reference Include=\"{Program.AppName}\">
                         writer.WriteLine($"<HintPath>../../../{Program.AppName}.dll</HintPath>
                         <Private>False</Private>
                         </Reference>
                         
                         <Reference Include=\"MonoGame.Framework\">
                         <HintPath>../../../MonoGame.Framework.dll</HintPath>
                         <Private>False</Private>
                         </Reference>
                         
                         <Reference Include=\"Serilog\">
                         <HintPath>../../../Serilog.dll</HintPath>
                         <Private>False</Private>
                         </Reference>
                         
                         <Reference Include=\"0Harmony\">
                         <HintPath>../../../0Harmony.dll</HintPath>
                         <Private>False</Private>
                         </Reference>
                         """);
    }

    /// <summary>
    /// Write project tasks, must be used at Project level
    /// </summary>
    /// <param name="writer">Writer which should write tasks</param>
    private void WriteProjectTasks(StreamWriter writer)
    {
        writer.WriteLine("""
                         <Target Name="CopyFiles" AfterTargets="Build">
                            <Copy SourceFiles="$(OutputPath)/$(AssemblyName).dll" DestinationFolder="../bin" />
                         </Target> 
                         """);
    }
}
