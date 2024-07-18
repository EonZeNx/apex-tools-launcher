using System;
using System.IO;
using ApexFormat.RTPC.V0104;
using ApexFormat.RTPC.V01;
using ApexFormat.SARC.V02;
using ApexFormat.TAB.V02;
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
        
        // Console hash should disrespect auto-close
        // Should be before it
        if (args.Length == 0)
        {
            ConsoleHash.Start();
            return;
        }
        
        if (CoreAppConfig.Get().PreloadHashes)
        {
            ConsoleLibrary.Log("Loading hashes into memory...", LogType.Info);
            LookupHashes.LoadAll();
        }
        AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;
        
        // TODO: Parse args and action files
        foreach (var arg in args)
        {

            var fileName = Path.GetFileName(arg);
            if (Path.Exists(arg))
            {
                ConsoleLibrary.Log($"Processing '{fileName}'", LogType.Info);
            }
            else
            {
                ConsoleLibrary.Log($"'{fileName}' does not exist", LogType.Warning);
                continue;
            }

            // SARCv02 test
            // var inBuffer = new FileStream(arg, FileMode.Open);
            //
            // var fileName = Path.GetFileNameWithoutExtension(arg);
            // var directoryPath = Path.Join(Path.GetDirectoryName(arg), fileName);
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
            
            // RTPCv0104
            // var inBuffer = new FileStream(arg, FileMode.Open);
            //
            // var targetFilePath = Path.GetDirectoryName(arg);
            // var targetFileName = Path.GetFileNameWithoutExtension(arg);
            // var targetXmlFilePath = Path.Join(targetFilePath, $"{targetFileName}.xml");
            // var outBuffer = new FileStream(targetXmlFilePath, FileMode.Create);
            //
            // RtpcV0104Manager.Decompress(inBuffer, outBuffer);
            
            // TABv02 test
            // var tabBuffer = new FileStream(arg, FileMode.Open);
            //
            // var fileName = Path.GetFileNameWithoutExtension(arg);
            // var directoryPath = Path.GetDirectoryName(arg);
            // if (!Directory.Exists(directoryPath)) continue;
            //
            // var arcPath = Path.Join(directoryPath, $"{fileName}.arc");
            // var arcBuffer = new FileStream(arcPath, FileMode.Open);
            //
            // var result = TabV02Manager.Decompress(tabBuffer, arcBuffer, directoryPath);
            // if (result < 0)
            // {
            //     throw new Exception();
            // }
            
            // RTPCv01
            var inBuffer = new FileStream(arg, FileMode.Open);
            
            var targetFilePath = Path.GetDirectoryName(arg);
            var targetFileName = Path.GetFileNameWithoutExtension(arg);
            var targetXmlFilePath = Path.Join(targetFilePath, $"{targetFileName}.xml");
            var outBuffer = new FileStream(targetXmlFilePath, FileMode.Create);
            
            RtpcV01Manager.Decompress(inBuffer, outBuffer);
            
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