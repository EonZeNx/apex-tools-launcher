using System.Collections.Generic;
using System.Xml.Linq;
using ATL.CLI.Script.Libraries;
using ATL.CLI.Script.Variables;
using ATL.Core.Extensions;
using ATL.Core.Libraries;

namespace ATL.CLI.Script.Actions;

public class ScriptActionPrint : IScriptAction
{
    public const string NodeName = "print";
    public string Format(string message) => $"{NodeName.ToUpper()}: {message}";
    
    public ScriptProcessResult Process(XElement node, Dictionary<string, IScriptVariable> parentVars)
    {
        var valueAttr = node.Attribute("value");
        if (valueAttr is null)
            return ScriptProcessResult.Error(Format("value attribute missing"));

        var value = ScriptLibrary.InterpolateString(valueAttr.Value, parentVars);
        
        var consoleColor = node.GetConsoleColor();
        ConsoleLibrary.Log(value, consoleColor);
        
        return ScriptProcessResult.Ok();
    }
}
