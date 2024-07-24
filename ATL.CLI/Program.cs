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
        CoreConfig.LoadAppConfig();
        
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

    public static string[] FilterUnsupportedPaths(IEnumerable<string> inPaths)
    {
        var supportedPaths = new List<string>();
        var pathsToCheck = inPaths.ToList();
        
        for (var i = 0; i < pathsToCheck.Count; i++)
        {
            var inputPath = pathsToCheck[i];

            try
            {
                if (!Path.Exists(inputPath))
                    continue;

                if (TabV02Manager.CanProcess(inputPath) ||
                    SarcV02Manager.CanProcess(inputPath) ||
                    AafV01Manager.CanProcess(inputPath) ||
                    RtpcV01Manager.CanProcess(inputPath) ||
                    RtpcV0104Manager.CanProcess(inputPath)
                ) {
                    supportedPaths.Add(inputPath);
                    continue;
                }

                if (Directory.Exists(inputPath))
                { // directory unsupported, try process child files
                    pathsToCheck.AddRange(Directory.GetFiles(inputPath, "*", SearchOption.TopDirectoryOnly));
                }
            }
            catch (Exception e)
            {
                ConsoleLibrary.Log($"Failed to check '{inputPath}'", ConsoleColor.Yellow);
                ConsoleLibrary.Log($"{e}: {e.Message}", ConsoleColor.Red);
            }
        }

        return supportedPaths.ToArray();
    }

    public static void MainWithOptions(AtlClOptions inOptions)
    {
        var options = (AtlClOptions) inOptions.Clone();

        if (options.AutoClose >= 0)
            CoreConfig.AppConfig.Cli.AutoClose = options.AutoClose != 0;

        var targetDatabases = options.TargetDatabases.ToArray();
        if (targetDatabases.Length != 0)
        {
            foreach (var targetDatabase in targetDatabases)
            {
                HashDatabases.OpenConnection(targetDatabase);
            }

            CoreConfig.AppConfig.PreloadHashes = true;
        }
        
        if (CoreConfig.AppConfig.PreloadHashes)
        {
            ConsoleLibrary.Log("Loading all hashes into memory...", LogType.Info);
            HashDatabases.LoadAll();
        }
        
        if (!string.IsNullOrEmpty(options.OutputDirectory))
        {
            if (!Directory.Exists(options.OutputDirectory))
                Directory.CreateDirectory(options.OutputDirectory);
        }

        var paths = FilterUnsupportedPaths(options.InputPaths);
        
#if DEBUG
        for (var i = 0; i < paths.Length; i++)
        {
            OperateFile(paths[i], options.OutputDirectory);
        }
#else
        Parallel.For(0, paths.Length, i =>
        {
            OperateFile(paths[i], options.OutputDirectory);
        });
#endif
    }

    public static void MainWithErrors(ParserResult<AtlClOptions> result, IEnumerable<Error> errors)
    {
        CoreConfig.AppConfig.Cli.AutoClose = false;
        
        var helpText = HelpText.AutoBuild(result, h =>
        {
            h.AdditionalNewLineAfterOption = false;
            h.Heading = $"{ConstantsLibrary.AppFullTitle} {ConstantsLibrary.AppVersion}";

            return HelpText.DefaultParsingErrorsHandler(result, h);
        }, e => e);
        
        ConsoleLibrary.Log(helpText, ConsoleColor.White);
    }

    public static string GetAbsoluteDirectory(string inPath, string outDirectory)
    {
        var result = Path.GetDirectoryName(inPath) ?? inPath;
        
        if (!string.IsNullOrEmpty(outDirectory))
        { // outDirectory is valid
            result = Path.IsPathFullyQualified(outDirectory)
                ? outDirectory
                : Path.GetFullPath(outDirectory, AppDomain.CurrentDomain.BaseDirectory);
        }

        return result;
    }
    
    public static void OperateFile(string inPath, string outDirectory)
    {
        var pathName = Path.GetFileName(inPath);
        if (string.IsNullOrEmpty(pathName))
            pathName = Path.GetDirectoryName(inPath);
        
        var message = $"Processing '{pathName}'";

        IProcessBasic manager;
        if (TabV02Manager.CanProcess(inPath))
        {
            manager = new TabV02Manager();
            message = $"{message} as TABv02";
        }
        else if (SarcV02Manager.CanProcess(inPath))
        {
            manager = new SarcV02Manager();
            message = $"{message} as SARCv02";
        }
        else if (AafV01Manager.CanProcess(inPath))
        {
            manager = new AafV01Manager();
            message = $"{message} as AAFv01";
        }
        else if (RtpcV01Manager.CanProcess(inPath))
        {
            manager = new RtpcV01Manager();
            message = $"{message} as RTPCv01";
        }
        else if (RtpcV0104Manager.CanProcess(inPath))
        { // should be last
            manager = new RtpcV0104Manager();
            message = $"{message} as RTPCv0104";
        }
        else
        {
            ConsoleLibrary.Log($"File not supported '{pathName}'", LogType.Warning);
            return;
        }
        
        ConsoleLibrary.Log(message, LogType.Info);
        
        var absoluteOutDirectory = GetAbsoluteDirectory(inPath, outDirectory);
        manager.ProcessBasic(inPath, absoluteOutDirectory);
        
        ConsoleLibrary.Log($"Finished '{pathName}'", LogType.Info);
    }
    
    public static void CurrentDomain_ProcessExit(object? sender, EventArgs e)
    {
        if (!CoreConfig.AppConfig.Cli.AutoClose)
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