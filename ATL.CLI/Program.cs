using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ApexFormat.AAF.V01;
using ApexFormat.RTPC.V0104;
using ApexFormat.RTPC.V01;
using ApexFormat.SARC.V02;
using ApexFormat.TAB.V02;
using ATL.CLI.Console;
using ATL.Core.Class;
using ATL.Core.Config;
using ATL.Core.Hash;
using ATL.Core.Libraries;

namespace ATL.CLI;

class Program
{
    static void Main(string[] args)
    {
#if !DEBUG
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
#endif
        CoreAppConfig.LoadAppConfig();
        
        if (args.Length == 0)
        {
            AtlConsole.Loop();
            Close();
        }
        
        AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;
        if (CoreAppConfig.Get().PreloadHashes)
        {
            ConsoleLibrary.Log("Loading all hashes into memory...", LogType.Info);
            HashDatabase.LoadAll();
        }
        
        var pathArgs = args.Where(path => Path.Exists(path) && !path.EndsWith(".exe")).ToArray();
        Parallel.For(0, pathArgs.Length, i =>
        {
            OperateFiles(pathArgs[i]);
        });
        
        Close();
    }

    public static void OperateFiles(string filePath)
    {
        var fileName = Path.GetFileName(filePath);
        ConsoleLibrary.Log($"Processing '{fileName}'", LogType.Info);

        if (TabV02Manager.CanProcess(filePath))
        {
            var manager = new TabV02Manager();
            manager.ProcessBasic(filePath);
        }
        else if (SarcV02Manager.CanProcess(filePath))
        {
            var manager = new SarcV02Manager();
            manager.ProcessBasic(filePath);
        }
        else if (AafV01Manager.CanProcess(filePath))
        {
            var manager = new AafV01Manager();
            manager.ProcessBasic(filePath);
        }
        else if (RtpcV01Manager.CanProcess(filePath))
        {
            var manager = new RtpcV01Manager();
            manager.ProcessBasic(filePath);
        }
        else if (RtpcV0104Manager.CanProcess(filePath))
        {
            var manager = new RtpcV0104Manager();
            manager.ProcessBasic(filePath);
        }
            
        ConsoleLibrary.Log($"Finished '{fileName}'", LogType.Info);
    }
    
    public static void CurrentDomain_ProcessExit(object? sender, EventArgs e)
    {
        if (!CoreAppConfig.Get().Cli.AutoClose)
        {
            ConsoleLibrary.GetInput("Press any key to continue...");
        }
    }
    
    public static void CurrentDomain_UnhandledException(object? sender, UnhandledExceptionEventArgs e)
    {
        var exception = (Exception) e.ExceptionObject;
        
        ConsoleLibrary.Log($"{exception}: {exception.Message}", LogType.Error);
        ConsoleLibrary.GetInput("Press any key to continue...");
        
        Environment.Exit(-1);
    }
    
    public static void Close(string message = "")
    {
        if (!string.IsNullOrEmpty(message))
        {
            ConsoleLibrary.Log(message, LogType.Warning);
        }
        
        ConsoleLibrary.Log("exiting...", LogType.Info);
        Environment.Exit(0);
    }
}