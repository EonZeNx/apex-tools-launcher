using System.Collections.Generic;
using System.Xml.Linq;
using ATL.CLI.Script.Libraries;
using ATL.CLI.Script.Variables;

namespace ATL.CLI.Script.Actions;

public class ScriptActionBreak : IScriptAction
{
    public const string NodeName = "break";
    public string Format(string message) => $"{NodeName.ToUpper()}: {message}";

    public ScriptProcessResult Process(XElement node, Dictionary<string, IScriptVariable> parentVars)
    {
        return ScriptProcessResult.OkBreak();
    }
}
