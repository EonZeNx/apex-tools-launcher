using System.Xml.Linq;
using ATL.Script.Libraries;
using ATL.Script.Variables;

namespace ATL.Script.Actions;

public class ScriptActionBreak : IScriptAction
{
    public const string NodeName = "break";
    public string Format(string message) => $"{NodeName.ToUpper()}: {message}";

    public ScriptProcessResult Process(XElement node, Dictionary<string, IScriptVariable> parentVars)
    {
        return ScriptProcessResult.OkBreak();
    }
}
