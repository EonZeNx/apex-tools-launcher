using System.Xml.Linq;
using ATL.Script.Variables;

namespace ATL.Script.Actions;

public class ScriptActionRename : IScriptAction
{
    public static string NodeName { get; } = "rename";
    
    public void Process(XElement node, Dictionary<string, ScriptVariable> parentVars)
    {
        var scriptActionMove = new ScriptActionMove();
        scriptActionMove.Process(node, parentVars);
    }
}
