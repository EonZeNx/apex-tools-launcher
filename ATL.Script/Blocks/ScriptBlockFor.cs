using System.Xml.Linq;
using ATL.Core.Libraries;
using ATL.Script.Libraries;
using ATL.Script.Variables;

namespace ATL.Script.Blocks;

public class ScriptBlockFor : IScriptBlock
{
    public const string NodeName = "for";
    public string Format(string message) => $"{NodeName.ToUpper()}: {message}";

    public Dictionary<string, IScriptVariable> Variables { get; set; } = new();

    public ScriptProcessResult Loop(string value, string varName,XElement node, Dictionary<string, IScriptVariable> parentVars)
    {
        var valueVariable = new ScriptVariable
        {
            Name = varName,
            Type = EScriptVariableType.String,
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
        var result = scriptBlock.Process(node, allVariables);
        
        return result;
    }
    
    public ScriptProcessResult Process(XElement node, Dictionary<string, IScriptVariable> parentVars)
    {
        var nameAttr = node.Attribute("name");
        if (nameAttr is null)
            return ScriptProcessResult.Error(Format("name attribute missing"));

        var blockName = ScriptLibrary.InterpolateString(nameAttr.Value, parentVars);
        
        var eachAttr = node.Attribute("each");
        if (eachAttr is null)
            return ScriptProcessResult.Error(Format("each attribute missing"));

        var eachVarName = ScriptLibrary.StripSymbol(eachAttr.Value);
        if (!parentVars.TryGetValue(eachVarName, out var eachVar))
            return ScriptProcessResult.Error(Format($"eachVarName '{eachVarName}' variable not found"));

        var varName = $"{blockName}_{eachVarName}";
        
        var optionValues = eachVar.As<List<string>>();
        if (!optionValues.IsSome(out var values))
            return ScriptProcessResult.Error(Format("values failed to cast"));

        var parallel = false;
        var parallelAttr = node.Attribute("parallel");
        if (parallelAttr is not null)
        {
            if (int.TryParse(parallelAttr.Value, out var number))
            {
                parallel = number != 0;
            }
        }

        var result = ScriptProcessResult.Ok();
        if (parallel)
        {
            Parallel.For(0, values.Count, (i, state) =>
            {
                var subResult = Loop(values[i], varName, node, parentVars);
                if (subResult.Type >= 0 && subResult.Type != EScriptProcessResultType.Break)
                    return;

                if (subResult.Type != EScriptProcessResultType.Break)
                {
                    lock (result)
                    {
                        subResult.Copy(result);
                    }
                }
                
                state.Break();
            });
        }
        else
        {
            foreach (var value in values)
            {
                var subResult = Loop(value, varName, node, parentVars);
                if (subResult.Type is < 0 or EScriptProcessResultType.Break)
                {
                    if (subResult.Type != EScriptProcessResultType.Break)
                    {
                        subResult.Copy(result);
                    }
                    
                    break;
                }
            }
        }

        return result;
    }
}