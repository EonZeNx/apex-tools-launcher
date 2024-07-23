using System;
using System.Collections.Generic;
using CommandLine;

namespace ATL.CLI;

public class AtlClOptions : ICloneable
{
    // arbitrary max length for command line parser
    public const int MaxFilePaths = 2048;
    
    [Value(0, Min = 1, Max = MaxFilePaths)]
    public IEnumerable<string> FilePaths { get; set; } = [];
    
    [Option('o', "outputDirectory", HelpText = "output directory")]
    public string OutputDirectory { get; set; } = "";

    public object Clone()
    {
        var result = new AtlClOptions
        {
            FilePaths = FilePaths,
            OutputDirectory = OutputDirectory
        };

        return result;
    }
}