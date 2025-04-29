using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using MonoPlus;
using Serilog;

namespace DerelictDimension;

/// <summary>
/// Entry class for the executable
/// </summary>
public static class Program
{
    /// <summary>
    /// Times the game was restarted after throwing an <see cref="Exception"/>
    /// </summary>
    public static int RestartsCount = 0;

    /// <summary>
    /// Max amount of times game should restart after throwing an <see cref="Exception"/>
    /// </summary>
    public static int MaxRestarts = 1;

    /// <summary>
    /// <see cref="File"/> path to file where error should be written
    /// </summary>
    public static string errorFile = $"{AppContext.BaseDirectory}error.txt";

    /// <summary>
    /// <see cref="File"/> path to file with run log
    /// </summary>
    public static string logFile = $"{AppContext.BaseDirectory}log.txt";

    /// <summary>
    /// <see cref="File"/> path to file which is created if the game fails to log an exception
    /// </summary>
    public static string errorx2File = $"{AppContext.BaseDirectory}ERRORX2.txt";

    /// <summary>
    /// Entry point of the executable
    /// </summary>
    public static void Main()
    {
        MonoPlusMain.EarlyInitialize();
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) WindowsAPI.AllocConsole();

        //DO NOT USE Main(string[]) ! That is different from Environment.GetCommandLineArgs(); because it doesn't include path to process executable as first arg, so to prevent confusion and messing up indexes use this Environment.GetCommandLineArgs(); instead.
        string[] args = Environment.GetCommandLineArgs();
        Log.Information("Command-line arguments: {Args}", string.Join(' ', args));
        File.Delete(errorx2File);
        File.WriteAllText(errorFile, $"{DateTime.Now}\n");
        File.Create(logFile);

        try
        {
            if (args.Length > 1)
                switch (args[1])
                {
                    case "mod":
                        ModsCLI.Run(args);
                        Environment.Exit(0);
                        break;
                }
        }
        catch (Exception exception)
        {
            Crash(exception);
            Environment.Exit(2);
        }
        

        while (RestartsCount < MaxRestarts)
        {
            try
            {
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
        
        try
        {
            File.WriteAllText(errorFile, "Too many restarts!");
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception.ToString());
            File.Create($"{AppContext.BaseDirectory}ERRORX2.txt");
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
            Log.Fatal(exception, "An exception was throw, restarting the program.");
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

