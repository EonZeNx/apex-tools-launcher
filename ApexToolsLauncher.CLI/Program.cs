﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ApexToolsLauncher.Core.Config;
using ApexToolsLauncher.Core.Hash;
using ApexToolsLauncher.Core.Libraries;
using CommandLine;
using CommandLine.Text;
#if !DEBUG
using System.Threading.Tasks;
#endif

namespace ApexToolsLauncher.CLI;

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

        var paths = inOptions.InputPaths.ToArray();
        
#if DEBUG
        foreach (var path in paths)
        {
            var managerOption = AtlOperate.GetOperator(path);
            if (!managerOption.IsSome(out var manager))
            {
                ConsoleLibrary.Log($"File not supported {path}", LogType.Warning);
                continue;
            }
            
            var pathName = Path.GetFileName(path);
            if (string.IsNullOrEmpty(pathName))
                pathName = Path.GetDirectoryName(path);

            ConsoleLibrary.Log($"Processing {pathName} as {manager.GetProcessorName()}", LogType.Info);

            AtlOperate.RunOperator(path, manager, options.OutputDirectory);

            ConsoleLibrary.Log($"Finished {pathName}", LogType.Info);
        }
#else
        Parallel.ForEach(paths, (path) =>
        {
            var managerOption = AtlOperate.GetOperator(path);
            if (!managerOption.IsSome(out var manager))
            {
                ConsoleLibrary.Log($"File not supported {path}", LogType.Warning);
                return;
            }
            
            var pathName = Path.GetFileName(path);
            if (string.IsNullOrEmpty(pathName))
                pathName = Path.GetDirectoryName(path);

            ConsoleLibrary.Log($"Processing {pathName} as {manager.GetProcessorName()}", LogType.Info);

            AtlOperate.RunOperator(path, manager, options.OutputDirectory);

            ConsoleLibrary.Log($"Finished {pathName}", LogType.Info);
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