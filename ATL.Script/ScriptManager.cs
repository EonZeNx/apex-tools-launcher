using System.Xml.Linq;
using ATL.Core.Class;
using ATL.Core.Libraries;
using ATL.Script.Blocks;
using ATL.Script.Libraries;
using ATL.Script.Variables;

namespace ATL.Script;

public class ScriptManager : IProcessBasic
{
    public void Load(string filepath)
    {
        if (!Path.Exists(filepath))
        {
            return;
        }

        var xDoc = XElement.Load(filepath);
        if (!xDoc.HasElements)
        {
            return;
        }

        var variables = new Dictionary<string, IScriptVariable>();
        
        var scriptBlock = new ScriptBlock();
        var result = scriptBlock.Process(xDoc, variables);
        
        if (result.ResultType != EScriptProcessResultType.Complete || !string.IsNullOrEmpty(result.Message))
        {
            var consoleColour = result.ResultType switch
            {
                EScriptProcessResultType.Error => ConsoleColor.Red,
                EScriptProcessResultType.Warning => ConsoleColor.Yellow,
                EScriptProcessResultType.Complete => ConsoleColor.Green,
                EScriptProcessResultType.Info => ConsoleColor.Cyan,
                EScriptProcessResultType.Break => ConsoleColor.White,
                _ => ConsoleColor.White
            };

            ConsoleLibrary.Log(result.Message, consoleColour);
        }
    }

    public int ProcessBasic(string inFilePath, string outDirectory)
    {
        Load(inFilePath);
        
        return 0;
    }
}