using System;
using System.Collections.Generic;
using System.Xml.Linq;
using ATL.CLI.Script.Blocks;
using ATL.CLI.Script.Libraries;
using ATL.CLI.Script.Variables;
using RustyOptions;

namespace ATL.CLI.Script.Operations;

public class ScriptOperationsString : IScriptBlock, IScriptVariable
{
    public const string NodeName = "string";
    public string Format(string message) => $"{NodeName.ToUpper()}: {message}";
    
    public Dictionary<string, IScriptVariable> Variables { get; set; } = new();

    public string Name { get; set; } = "UNSET";
    public EScriptVariableType Type { get; set; } = EScriptVariableType.String;
    public EScriptVariableMetaType MetaType { get; set; } = EScriptVariableMetaType.Normal;
    public object? Data { get; set; } = null;
    
    public Option<T> As<T>() where T : notnull
    {
        return Data is null
            ? Option<T>.None
            : Option.Some((T) Data);
    }

    public string OperateSplit(string data, XElement node, Dictionary<string, IScriptVariable> parentVars)
    {
        var take = 0;
        var takeAttr = node.Attribute("take");
        if (takeAttr is not null)
        {
            if (int.TryParse(takeAttr.Value, out var number))
            {
                take = number;
            }
        }
        
        var symbol = @"\";
        var symbolAttr = node.Attribute("symbol");
        if (symbolAttr is not null)
        {
            symbol = symbolAttr.Value;
        }
        
        var splitValue = data.Split(symbol);
        if (take > splitValue.Length || take < -(splitValue.Length - 1))
            return splitValue[0];
        
        var result = take >= 0
            ? splitValue[take]
            : splitValue[^Math.Abs(take)];
        return result;
    }
    
    public ScriptProcessResult Process(XElement node, Dictionary<string, IScriptVariable> parentVars)
    {
        var nameAttr = node.Attribute("name");
        if (nameAttr is null)
            return ScriptProcessResult.Error(Format("name attribute missing"));
        Name = nameAttr.Value;

        var targetVarAttr = node.Attribute("target");
        if (targetVarAttr is null)
            return ScriptProcessResult.Error(Format("target attribute missing"));

        var targetVarName = ScriptLibrary.StripSymbol(targetVarAttr.Value);
        if (!parentVars.TryGetValue(targetVarName, out var targetVar))
            return ScriptProcessResult.Error(Format($"targetVarName '{targetVarName}' variable not found"));
        
        var optionData = targetVar.As<string>();
        if (!optionData.IsSome(out var data))
            return ScriptProcessResult.Error(Format("targetVar failed to cast"));

        foreach (var element in node.Elements())
        {
            var xeName = element.Name.ToString();

            data = xeName switch
            {
                "split" => OperateSplit(data, element, parentVars),
                _ => data
            };
        }
        
        Data = data;
        return ScriptProcessResult.Ok();
    }
}