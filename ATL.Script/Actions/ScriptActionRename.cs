using System.Xml.Linq;
using ATL.Script.Variables;

namespace ATL.Script.Actions;

public class ScriptActionRename : IScriptAction
{
    public static string NodeName { get; } = "rename";
    
    public void Process(XElement element, Dictionary<string, ScriptVariable> variables)
    {
        var scriptActionMove = new ScriptActionMove();
        scriptActionMove.Process(element, variables);
    }
}
