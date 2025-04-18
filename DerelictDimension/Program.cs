﻿using System;
using System.IO;
using Serilog;

namespace DerelictDimension;

public static class Program
{
    public static int RestartsCount = 0;
    public static int MaxRestarts = 1;
    public static string errorFile = $"{AppContext.BaseDirectory}error.txt";
    public static string logFile = $"{AppContext.BaseDirectory}log.txt";

    public static void Main(string[] args)
    {
        while (RestartsCount < MaxRestarts)
        {
            try
            {
                File.Delete(errorFile);
                File.Delete(logFile);
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
                    try
                    {
                        File.Create(errorFile);
                        File.WriteAllText(errorFile, $"{exception}\n\n\n{exception2}");
                    }
                    catch (Exception exception3)
                    {
                        Console.WriteLine(exception.ToString());
                        Console.WriteLine(exception2.ToString());
                        Console.WriteLine(exception3.ToString());
                        //this can't throw an exception, right?
                    }

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

