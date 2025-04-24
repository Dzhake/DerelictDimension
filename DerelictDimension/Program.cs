using System;
using System.IO;
using Serilog;

namespace DerelictDimension;

public static class Program
{
    public static int RestartsCount = 0;
    public static int MaxRestarts = 1;
    public static string errorFile = $"{AppContext.BaseDirectory}error.txt";
    public static string logFile = $"{AppContext.BaseDirectory}log.txt";
    public static string errorx2File = $"{AppContext.BaseDirectory}ERRORX2.txt";

    public static void Main(string[] args)
    {
        File.Delete(errorx2File);
        File.Create(errorFile); //clears files
        File.Create(logFile);

        while (RestartsCount < MaxRestarts)
        {
            try
            {
                RunGame();
            }
            catch (Exception exception)
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

    public static void RunGame()
    {
        new Engine().Run();
    }
}

