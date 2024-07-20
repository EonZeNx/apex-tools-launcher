using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ApexFormat.RTPC.V0104;
using ApexFormat.RTPC.V01;
using ApexFormat.SARC.V02;
using ApexFormat.TAB.V02;
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
            AtlCli.Loop();
            Close();
        }
        
        AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;
        if (CoreAppConfig.Get().PreloadHashes)
        {
            ConsoleLibrary.Log("Loading all hashes into memory...", LogType.Info);
            HashDatabase.LoadAll();
        }
        
        var pathArgs = args.Where(path => Path.Exists(path) && !path.EndsWith(".exe"));
        foreach (var pathArg in pathArgs)
        {
            var fileName = Path.GetFileName(pathArg);
            ConsoleLibrary.Log($"Processing '{fileName}'", LogType.Info);

            // SARCv02 test
            // var inBuffer = new FileStream(pathArg, FileMode.Open);
            //
            // var fileName = Path.GetFileNameWithoutExtension(pathArg);
            // var directoryPath = Path.Join(Path.GetDirectoryName(pathArg), fileName);
            //
            // if (!Directory.Exists(directoryPath))
            // {
            //     Directory.CreateDirectory(directoryPath);
            // }
            //
            // var result = SarcV02Manager.Decompress(inBuffer, directoryPath);
            // if (result < 0)
            // {
            //     throw new Exception();
            // }
            
            // TABv02 test
            // var tabBuffer = new FileStream(pathArg, FileMode.Open);
            //
            // var directoryPath = Path.GetDirectoryName(pathArg);
            // if (!Directory.Exists(directoryPath)) continue;
            //
            // var fileNameWoExtension = Path.GetFileNameWithoutExtension(pathArg);
            // var arcPath = Path.Join(directoryPath, $"{fileNameWoExtension}.arc");
            // using var arcBuffer = new FileStream(arcPath, FileMode.Open);
            //
            // var result = TabV02Manager.Decompress(tabBuffer, arcBuffer, directoryPath);
            // if (result < 0)
            // {
            //     throw new Exception();
            // }
            
            // RTPCv0104
            var inBuffer = new FileStream(pathArg, FileMode.Open);
            
            var targetFilePath = Path.GetDirectoryName(pathArg);
            var targetFileName = Path.GetFileNameWithoutExtension(pathArg);
            var targetXmlFilePath = Path.Join(targetFilePath, $"{targetFileName}.xml");
            var outBuffer = new FileStream(targetXmlFilePath, FileMode.Create);
            
            RtpcV0104Manager.Decompress(inBuffer, outBuffer);
            
            // RTPCv01
            // var inBuffer = new FileStream(pathArg, FileMode.Open);
            //
            // var targetFilePath = Path.GetDirectoryName(pathArg);
            // var targetFileName = Path.GetFileNameWithoutExtension(pathArg);
            // var targetXmlFilePath = Path.Join(targetFilePath, $"{targetFileName}.xml");
            // var outBuffer = new FileStream(targetXmlFilePath, FileMode.Create);
            //
            // RtpcV01Manager.Decompress(inBuffer, outBuffer);
            
            
            ConsoleLibrary.Log($"Finished '{fileName}'", LogType.Info);
        }
        
        Close();
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