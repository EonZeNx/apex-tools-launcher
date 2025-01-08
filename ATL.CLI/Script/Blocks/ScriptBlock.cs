using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using ATL.CLI.Script.Actions;
using ATL.CLI.Script.Libraries;
using ATL.CLI.Script.Operations;
using ATL.CLI.Script.Queries;
using ATL.CLI.Script.Variables;

namespace ATL.CLI.Script.Blocks;

public class ScriptBlock : IScriptBlock
{
    public string Format(string message) => $"block: {message}";
    public Dictionary<string, IScriptVariable> Variables { get; set; } = new();
    
    public virtual ScriptProcessResult Process(XElement node, Dictionary<string, IScriptVariable> parentVars)
    {
        var result = new ScriptProcessResult();
        
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
                ScriptVariableAsk.NodeName => new ScriptVariableAsk(),
                ScriptActionCopy.NodeName => new ScriptActionCopy(),
                ScriptActionRename.NodeName => new ScriptActionRename(),
                ScriptActionMove.NodeName => new ScriptActionMove(),
                ScriptActionDelete.NodeName => new ScriptActionDelete(),
                ScriptActionProcess.NodeName => new ScriptActionProcess(),
                ScriptActionPrint.NodeName => new ScriptActionPrint(),
                ScriptActionBreak.NodeName => new ScriptActionBreak(),
                ScriptQuery.NodeName => new ScriptQuery(),
                ScriptOperationsString.NodeName => new ScriptOperationsString(),
                ScriptBlockFor.NodeName => new ScriptBlockFor(),
                _ => null
            };

            if (scriptAction is null)
            {
                result.ResultType = EScriptProcessResultType.Warning;
                continue;
            };
            
            var subResult = scriptAction.Process(element, allVariables);
            if (subResult.ResultType is EScriptProcessResultType.Break or EScriptProcessResultType.Error)
            {
                result = subResult;
                break;
            }

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
                
                var optionNewData = scriptVariable.As<List<string>>();
                if (!optionNewData.IsSome(out var newData))
                    continue;
                
                existingData.AddRange(newData);
                Variables[scriptVariable.Name].Data = existingData;
            }
        }
        
        return result;
    }
}