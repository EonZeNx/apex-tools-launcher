using System;
using System.Collections.Generic;
using CommandLine;

namespace ApexToolsLauncher.CLI;

public class AtlClOptions : ICloneable
{
    // arbitrary max length for command line parser
    public const int MaxInputPathsCount = 2048;
    public const int MaxDatabaseCount = 128;
    
    [Value(0, Min = 1, Max = MaxInputPathsCount, MetaName = "input paths", HelpText = "list of paths to process. absolute path")]
    public IEnumerable<string> InputPaths { get; set; } = Array.Empty<string>();
    
    [Option('o', "out", HelpText = "output directory. absolute path")]
    public string OutputDirectory { get; set; } = "";
    
    [Option('d', "databases", Min = 1, Max = MaxDatabaseCount, HelpText = "use a selection of databases. absolute path")]
    public IEnumerable<string> TargetDatabases { get; set; } = Array.Empty<string>();
    
    [Option('c', "autoclose", HelpText = "override auto-close. 0 = disabled, 1 = enabled")]
    public int AutoClose { get; set; } = -1;

    public object Clone()
    {
        var result = new AtlClOptions
        {
            InputPaths = InputPaths,
            OutputDirectory = OutputDirectory,
            TargetDatabases = TargetDatabases,
            AutoClose = AutoClose,
        };

        return result;
    }
}