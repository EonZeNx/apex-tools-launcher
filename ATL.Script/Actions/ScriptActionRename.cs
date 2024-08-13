using System.Xml.Linq;
using ATL.Script.Variables;

namespace ATL.Script.Actions;

public class ScriptActionRename : IScriptAction
{
    public const string NodeName = "rename";
    
    public void Process(XElement node, Dictionary<string, IScriptVariable> parentVars)
    {
        var scriptActionMove = new ScriptActionMove();
        scriptActionMove.Process(node, parentVars);
    }
}
