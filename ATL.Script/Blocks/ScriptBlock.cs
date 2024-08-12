using System.Xml.Linq;
using ATL.Core.Libraries;
using ATL.Script.Actions;
using ATL.Script.Variables;

namespace ATL.Script.Blocks;

public class ScriptBlock : IScriptBlock
{
    public Dictionary<string, ScriptVariable> Variables { get; set; } = new();
    
    public virtual void Process(XElement node, Dictionary<string, ScriptVariable> parentVars)
    {
        foreach (var element in node.Elements())
        {
            var xeName = element.Name.ToString();
            var allVariables = new Dictionary<string, ScriptVariable>();
            Variables.ToList()
                .ForEach(kvp => allVariables.TryAdd(kvp.Key, kvp.Value));
            parentVars.ToList()
                .ForEach(kvp => allVariables.TryAdd(kvp.Key, kvp.Value));
            
            if (xeName == ScriptVariable.NodeName)
            {
                var optionVar = element.GetScriptVariable(allVariables);
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
            else if (xeName == ScriptActionReplace.NodeName)
            {
                scriptAction = new ScriptActionReplace();
            }
            else if (xeName == ScriptActionProcess.NodeName)
            {
                scriptAction = new ScriptActionProcess();
            }
            else if (xeName == ScriptBlockFile.NodeName)
            {
                scriptAction = new ScriptBlockFile();
            }

            if (scriptAction is null)
                continue;
            
            scriptAction.Process(element, allVariables);
        }
    }
}