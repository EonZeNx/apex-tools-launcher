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
using CommandLine;
using CommandLine.Text;

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

        var optionParser = new CommandLine.Parser(s => s.HelpWriter = null);
        var options = optionParser.ParseArguments<AtlClOptions>(args);
        options
            .WithParsed(MainWithOptions)
            .WithNotParsed(e => MainWithErrors(options, e));
        
        Close();
    }

    public static void MainWithOptions(AtlClOptions inOptions)
    {
        var options = (AtlClOptions) inOptions.Clone();
        
        if (CoreAppConfig.Get().PreloadHashes)
        {
            ConsoleLibrary.Log("Loading all hashes into memory...", LogType.Info);
            HashDatabase.LoadAll();
        }
        
        if (!string.IsNullOrEmpty(options.OutputDirectory))
        {
            if (!Directory.Exists(options.OutputDirectory))
                Directory.CreateDirectory(options.OutputDirectory);
        }
        
        var pathArgs = options.FilePaths.Where(path => Path.Exists(path) && !path.EndsWith(".exe")).ToArray();
        Parallel.For(0, pathArgs.Length, i =>
        {
            OperateFile(pathArgs[i], options.OutputDirectory);
        });
    }

    public static void MainWithErrors(ParserResult<AtlClOptions> result, IEnumerable<Error> errors)
    {
        var helpText = HelpText.AutoBuild(result, h =>
        {
            h.AdditionalNewLineAfterOption = false;
            h.Heading = $"{ConstantsLibrary.AppFullTitle} {ConstantsLibrary.AppVersion}";

            return HelpText.DefaultParsingErrorsHandler(result, h);
        }, e => e);
        
        ConsoleLibrary.Log(helpText, ConsoleColor.White);
    }

    public static void OperateFile(string filePath, string outDirectory)
    {
        var fileName = Path.GetFileName(filePath);
        var message = $"Processing '{fileName}'";

        IProcessBasic manager;
        if (TabV02Manager.CanProcess(filePath))
        {
            manager = new TabV02Manager();
            message = $"{message} as TABv02";
        }
        else if (SarcV02Manager.CanProcess(filePath))
        {
            manager = new SarcV02Manager();
            message = $"{message} as SARCv02";
        }
        else if (AafV01Manager.CanProcess(filePath))
        {
            manager = new AafV01Manager();
            message = $"{message} as AAFv01";
        }
        else if (RtpcV01Manager.CanProcess(filePath))
        {
            manager = new RtpcV01Manager();
            message = $"{message} as RTPCv01";
        }
        else if (RtpcV0104Manager.CanProcess(filePath))
        { // should be last
            manager = new RtpcV0104Manager();
            message = $"{message} as RTPCv0104";
        }
        else
        {
            ConsoleLibrary.Log($"File not supported '{fileName}'", LogType.Warning);
            return;
        }
        
        ConsoleLibrary.Log(message, LogType.Info);
        manager.ProcessBasic(filePath, outDirectory);
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
        
        ConsoleLibrary.Log("Exiting...", LogType.Info);
        Environment.Exit(0);
    }
}