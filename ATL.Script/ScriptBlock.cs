using System.Xml.Linq;
using ATL.Core.Libraries;
using ATL.Script.Actions;
using ATL.Script.Variables;

namespace ATL.Script;

public class ScriptBlock
{
    public Dictionary<string, ScriptVariable> Variables = new();
    
    public void Process(XElement node)
    {
        foreach (var element in node.Elements())
        {
            var xeName = element.Name.ToString();
            
            if (xeName == ScriptVariable.NodeName)
            {
                var optionVar = element.GetScriptVariable();
                if (!optionVar.IsSome(out var scriptVariable))
                {
                    ConsoleLibrary.Log("Failed to initialise variable", ConsoleColor.Red);
                    continue;
                }
                
                Variables.TryAdd(scriptVariable.Name, scriptVariable);
                continue;
            }

            IScriptAction? scriptAction = null;
            if (xeName == ScriptActionCopy.NodeName)
            {
                scriptAction = new ScriptActionCopy();
            }
            else if (xeName == ScriptActionRename.NodeName)
            {
                scriptAction = new ScriptActionRename();
            }
            else if (xeName == ScriptActionMove.NodeName)
            {
                scriptAction = new ScriptActionMove();
            }
            else if (xeName == ScriptActionDelete.NodeName)
            {
                scriptAction = new ScriptActionDelete();
            }

            scriptAction?.Process(element, Variables);
        }
    }
}