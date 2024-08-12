using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ApexChain.AAFSARC;
using ApexFormat.AAF.V01;
using ApexFormat.ADF.V04;
using ApexFormat.AVTX.V01;
using ApexFormat.IRTPC.V14;
using ApexFormat.RTPC.V01;
using ApexFormat.RTPC.V03;
using ApexFormat.SARC.V02;
using ApexFormat.TAB.V02;
using ATL.CLI.Console;
using ATL.Console;
using ATL.Core.Class;
using ATL.Core.Config;
using ATL.Core.Hash;
using ATL.Core.Libraries;
using ATL.Script;
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
                    AafV01SarcV02Manager.CanProcess(inputPath) ||
                    AdfV04Manager.CanProcess(inputPath) ||
                    AvtxV01Manager.CanProcess(inputPath) ||
                    RtpcV01Manager.CanProcess(inputPath) ||
                    RtpcV03Manager.CanProcess(inputPath) ||
                    IrtpcV14Manager.CanProcess(inputPath) ||
                    Path.GetExtension(inputPath) == ".xml"
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
        
        // if (Path.GetExtension(inPath) == ".xml")
        // {
        //     manager = new ScriptManager();
        //     message = $"{message} as Script";
        // }
        
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

    public static void OperateFile(string inPath, string outDirectory)
    {
        if (Path.GetExtension(inPath) == ".xml")
        {
            var manager = new ScriptManager();
            manager.ProcessBasic(inPath, outDirectory);
            
            return;
        }
        
        AtlOperate.OperateFile(inPath, outDirectory);
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