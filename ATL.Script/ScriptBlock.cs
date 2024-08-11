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
            if (element.Name.ToString() == ScriptVariable.NodeName)
            {
                var optionVar = element.GetScriptVariable();
                if (!optionVar.IsSome(out var scriptVariable))
                {
                    ConsoleLibrary.Log("Failed to initialise variable", ConsoleColor.Red);
                    continue;
                }
                
                Variables.TryAdd(scriptVariable.Name, scriptVariable);
            }
            else if (element.Name.ToString() == ScriptActionCopy.NodeName)
            {
                var scriptAction = new ScriptActionCopy();
                scriptAction.Process(element, Variables);
            }
        }
    }
}