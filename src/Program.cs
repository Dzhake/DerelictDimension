using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using DerelictDimension.CommandLine;
using Monod.LogSystem;
using Monod.LocalizationSystem;
using Monod.ModSystem;
using Monod;
using Serilog;

namespace DerelictDimension;

/// <summary>
/// Entry class for the executable.
/// </summary>
public static class Program
{
    /// <summary>
    /// Name of running application.
    /// </summary>
    public static string AppName = "DerelictDimension";

    /// <summary>
    /// <see cref="File"/> path to file where error should be written.
    /// </summary>
    public static readonly string errorFile = $"{AppContext.BaseDirectory}error.txt";

    /// <summary>
    /// <see cref="File"/> path to file which is created if the game fails to log an exception.
    /// </summary>
    public static readonly string errorx2File = $"{AppContext.BaseDirectory}ERRORX2.txt";

    /// <summary>
    /// Entry point of the executable. Acts as try/catch wrapper around <see cref="SafeMain"/>.
    /// </summary>
    public static void Main()
    {
        try
        {
            SafeMain();
        }
        catch (Exception exception)
        {
            Crash(exception);
            Environment.Exit(1);
        }
    }

    /// <summary>
    /// Entry point of executable, wrapped by <see cref="Main"/>. All exceptions here are caught and logged.
    /// </summary>
    public static void SafeMain()
    {
        //It's very important to erase those files early, in case crash will occur they always need to be up-to-date
        File.Delete(errorx2File);
        File.WriteAllText(errorFile, $"{DateTime.Now}\n");
        File.WriteAllText(LogHelper.LogFile, $"{DateTime.Now}\n");
        
        //DO NOT USE Main(string[]) ! That is different from Environment.GetCommandLineArgs(); because it doesn't include path to process executable as first arg. To prevent confusion and messing up indexes use Environment.GetCommandLineArgs(); instead.
        string[] args = Environment.GetCommandLineArgs();
        if (args.Contains("--help") && RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            WindowsAPI.AllocConsole();
        }
        CMD.Parse(args.Skip(1).ToArray()); //First arg is path to .exe/.dll, which crashes the parser :^)
        if (args.Contains("--help"))
        {
            Console.WriteLine("['--help' found, exiting the program]");
            Console.WriteLine("\nPress any key to exit..");
            Console.ReadKey();
            File.AppendAllText(LogHelper.LogFile, $"Command-line arguments: {args}\n['--help' found, exiting the program]");
            return; //Assume user doesn't want to launch the app, and wants help instead.
        }
        InitializeMonoPlus(args);
        InitializeMods();
        RunGame();
    }

    private static void InitializeMonoPlus(string[] args)
    {
        if (CommandLineArgs.Console && RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            WindowsAPI.AllocConsole();
            Log.Information("Allocated console for the game");
            Task.Run(ReadConsoleInput);
        }

        MonodMain.EarlyInitialize();
        //DO NOT LOG anything with level below Information until this is called! Otherwise, those lines will never be logged.
        LogHelper.SetMinimumLogLevel(CommandLineArgs.LogLevel);
        if (CommandLineArgs.Language is not null) Locale.CurrentLanguage = CommandLineArgs.Language;
        
        Log.Information("Command-line arguments: {Args}", string.Join(' ', args));
        LogHelper.WriteStartupInfo();
    }

    private static void InitializeMods()
    {
        if (Directory.Exists(ModManager.ModsDirectory))
        {
            ModManager.Initialize();
            ModManager.LoadMods();
        }
    }

    /// <summary>
    /// Logs the <paramref name="exception"/>. Call before quitting the program.
    /// </summary>
    /// <param name="exception"><see cref="Exception"/> to log.</param>
    public static void Crash(Exception exception)
    {
        try
        {
            Log.Fatal(exception, "An exception was thrown.");
            File.AppendAllText(errorFile, $"{exception}\n\n\n");

            //open log.txt
            new Process
            {
                StartInfo = new ProcessStartInfo(LogHelper.LogFile)
                {
                    UseShellExecute = true
                }
            }.Start();
        }
        catch (Exception exception2)
        {
            //DO NOT TRY/CATCH THIS! If the program exits without writing error file then error is here.
            File.AppendAllText(errorFile, $"{exception}\n\n\n{exception2}");

            Environment.Exit(2);
        }
    }

    /// <summary>
    /// Runs <see langword="new"/> <see cref="Engine"/>.
    /// </summary>
    public static void RunGame()
    {
        new Engine().Run();
    }

    /// <summary>
    /// Reads input from <see cref="Console"/> add adds it to <see cref="DevConsole.CommandsQueue"/>.
    /// </summary>
    public static void ReadConsoleInput()
    {
        while (true)
        {
            string? command = Console.ReadLine();
            switch (command)
            {
                case null:
                    continue;
                case "exit":
                    Environment.Exit(0);
                    break;
            }
            
            DevConsole.CommandsQueue.Enqueue(command);
        }
        // ReSharper disable once FunctionNeverReturns yep i know that's the point
    }
}

