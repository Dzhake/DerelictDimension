using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using DerelictDimension.ModsTool;
using MonoPlus;
using MonoPlus.Logging;
using MonoPlus.Modding;
using Serilog;

namespace DerelictDimension;

/// <summary>
/// Entry class for the executable
/// </summary>
public static class Program
{
    /// <summary>
    /// Name of running application
    /// </summary>
    public static string AppName = "DerelictDimension";

    /// <summary>
    /// Times the game was restarted after throwing an <see cref="Exception"/>
    /// </summary>
    public static int RestartsCount;

    /// <summary>
    /// Max amount of times game should restart after throwing an <see cref="Exception"/>
    /// </summary>
    public static int MaxRestarts = 1;

    /// <summary>
    /// <see cref="File"/> path to file where error should be written
    /// </summary>
    public static string errorFile = $"{AppContext.BaseDirectory}error.txt";

    /// <summary>
    /// <see cref="File"/> path to file which is created if the game fails to log an exception
    /// </summary>
    public static string errorx2File = $"{AppContext.BaseDirectory}ERRORX2.txt";

    /// <summary>
    /// Entry point of the executable
    /// </summary>
    public static void Main()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) WindowsAPI.AllocConsole();
        //DO NOT LOG anything with level below Information until this is called! Otherwise, logged lines will never be logged.
        MonoPlusMain.EarlyInitialize();

        //DO NOT USE Main(string[]) ! That is different from Environment.GetCommandLineArgs(); because it doesn't include path to process executable as first arg, so to prevent confusion and [messing up indexes] use this Environment.GetCommandLineArgs(); instead.
        string[] args = Environment.GetCommandLineArgs();
        Log.Information("Command-line arguments: {Args}", string.Join(' ', args));
        File.Delete(errorx2File);
        File.WriteAllText(errorFile, $"{DateTime.Now}\n");

        try
        {
            if (args.Length > 1)
                switch (args[1])
                {
                case "mod":
                    ModsCLI.Run(args.Skip(2).ToArray());
                    Environment.Exit(0);
                    break;
                }
        }
        catch (Exception exception)
        {
            Crash(exception);
            Environment.Exit(1);
        }

        LoggingHelper.WriteStartupInfo();

        while (RestartsCount < MaxRestarts)
        {
            try
            {
                ModManager.Initialize();
                RunGame();
            }
            catch (Exception exception)
            {
                Crash(exception);
                RestartsCount++;
                continue;
            }

            Environment.Exit(0); //Exit if no exception caught
        }

        //open log.txt
        new Process
        {
            StartInfo = new ProcessStartInfo(LoggingHelper.LogFile)
            {
                UseShellExecute = true
            }
        }.Start();

        if (MaxRestarts == 1) Environment.Exit(0);

        try
        {
            File.AppendAllText(errorFile, "Too many restarts");
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception.ToString());
            File.Create($"{AppContext.BaseDirectory}ERRORX2.txt");
            Environment.Exit(3);
        }
    }

    /// <summary>
    /// Logs the <paramref name="exception"/>. Call before quitting the program
    /// </summary>
    /// <param name="exception"><see cref="Exception"/> to <see cref="Log"/></param>
    public static void Crash(Exception exception)
    {
        try
        {
            Log.Fatal(exception, "An exception was thrown.");
            File.AppendAllText(errorFile, $"{exception}\n\n\n");
        }
        catch (Exception exception2)
        {
            //DO NOT TRY/CATCH THIS! If the program exits without writing error file then error is here.
            File.AppendAllText(errorFile, $"{exception}\n\n\n{exception2}");

            Environment.Exit(2);
        }
    }

    /// <summary>
    /// Runs <see langword="new"/> <see cref="Engine"/>
    /// </summary>
    public static void RunGame()
    {
        new Engine().Run();
    }
}

