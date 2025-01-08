using System.Xml.Linq;
using ATL.Script.Libraries;
using ATL.Script.Variables;

namespace ATL.Script.Blocks;

public class ScriptBlockFor : IScriptBlock
{
    public const string NodeName = "for";
    
    public Dictionary<string, IScriptVariable> Variables { get; set; } = new();
    
    public void Process(XElement node, Dictionary<string, IScriptVariable> parentVars)
    {
        var nameAttr = node.Attribute("name");
        if (nameAttr is null)
            return;

        var blockName = ScriptLibrary.InterpolateString(nameAttr.Value, parentVars);
        
        var eachAttr = node.Attribute("each");
        if (eachAttr is null)
            return;

        var eachVarName = ScriptLibrary.StripSymbol(eachAttr.Value);
        if (!parentVars.TryGetValue(eachVarName, out var eachVar))
            return;

        var optionValues = eachVar.As<List<string>>();
        if (!optionValues.IsSome(out var values))
            return;

        foreach (var value in values)
        {
            var valueVariable = new ScriptVariable
            {
                Name = $"{blockName}_{eachVarName}",
                Type = EScriptVariableType.Path,
                Data = value
            };

            Variables[valueVariable.Name] = valueVariable;
            
            // Do this every iteration as new variables can be added
            var allVariables = new Dictionary<string, IScriptVariable>();
            Variables.ToList()
                .ForEach(kvp => allVariables.TryAdd(kvp.Key, kvp.Value));
            parentVars.ToList()
                .ForEach(kvp => allVariables.TryAdd(kvp.Key, kvp.Value));
            
            var scriptBlock = new ScriptBlock();
            scriptBlock.Process(node, allVariables);
        }
    }
}