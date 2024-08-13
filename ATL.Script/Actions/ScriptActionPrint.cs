using System.Xml.Linq;
using ATL.Core.Extensions;
using ATL.Core.Libraries;
using ATL.Script.Libraries;
using ATL.Script.Variables;

namespace ATL.Script.Actions;

public class ScriptActionPrint : IScriptAction
{
    public const string NodeName = "print";
    
    public void Process(XElement node, Dictionary<string, IScriptVariable> parentVars)
    {
        var valueAttr = node.Attribute("value");
        if (valueAttr is null)
            return;

        var value = ScriptLibrary.InterpolateString(valueAttr.Value, parentVars);
        
        var consoleColor = node.GetConsoleColor();
        ConsoleLibrary.Log(value, consoleColor);
    }
}
