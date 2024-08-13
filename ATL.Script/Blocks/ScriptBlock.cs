using System.Xml.Linq;
using ATL.Script.Actions;
using ATL.Script.Queries;
using ATL.Script.Variables;

namespace ATL.Script.Blocks;

public class ScriptBlock : IScriptBlock
{
    public Dictionary<string, IScriptVariable> Variables { get; set; } = new();
    
    public virtual void Process(XElement node, Dictionary<string, IScriptVariable> parentVars)
    {
        foreach (var element in node.Elements())
        {
            var xeName = element.Name.ToString();
            
            // Do this every iteration as new variables can be added
            var allVariables = new Dictionary<string, IScriptVariable>();
            Variables.ToList()
                .ForEach(kvp => allVariables.TryAdd(kvp.Key, kvp.Value));
            parentVars.ToList()
                .ForEach(kvp => allVariables.TryAdd(kvp.Key, kvp.Value));

            IScriptAction? scriptAction = xeName switch
            {
                ScriptVariable.NodeName => new ScriptVariable(),
                ScriptActionCopy.NodeName => new ScriptActionCopy(),
                ScriptActionRename.NodeName => new ScriptActionRename(),
                ScriptActionMove.NodeName => new ScriptActionMove(),
                ScriptActionDelete.NodeName => new ScriptActionDelete(),
                ScriptActionProcess.NodeName => new ScriptActionProcess(),
                ScriptQuery.NodeName => new ScriptQuery(),
                ScriptActionPrint.NodeName => new ScriptActionPrint(),
                ScriptBlockFor.NodeName => new ScriptBlockFor(),
                ScriptBlockPath.NodeName => new ScriptBlockPath(),
                _ => null
            };
            
            if (scriptAction is null)
                continue;
            
            scriptAction.Process(element, allVariables);

            if (scriptAction is not IScriptVariable scriptVariable)
                continue;
            
            if (!Variables.TryGetValue(scriptVariable.Name, out var existingVar))
            {
                Variables.Add(scriptVariable.Name, scriptVariable);
                continue;
            }
            
            if (scriptVariable.MetaType == EScriptVariableMetaType.List
                && existingVar.MetaType == EScriptVariableMetaType.List)
            {
                var optionExistingData = existingVar.As<List<string>>();
                if (!optionExistingData.IsSome(out var existingData))
                    continue;
                
                var optionNewData = existingVar.As<List<string>>();
                if (!optionNewData.IsSome(out var newData))
                    continue;
                
                // TODO: untested, ensure this updates Variables map
                existingData.AddRange(newData);
            }
        }
    }
}