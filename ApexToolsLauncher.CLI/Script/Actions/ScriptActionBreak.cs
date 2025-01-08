using System.Collections.Generic;
using System.Xml.Linq;
using ApexToolsLauncher.CLI.Script.Libraries;
using ApexToolsLauncher.CLI.Script.Variables;

namespace ApexToolsLauncher.CLI.Script.Actions;

public class ScriptActionBreak : IScriptAction
{
    public const string NodeName = "break";
    public string Format(string message) => $"{NodeName.ToUpper()}: {message}";

    public ScriptProcessResult Process(XElement node, Dictionary<string, IScriptVariable> parentVars)
    {
        return ScriptProcessResult.OkBreak();
    }
}
