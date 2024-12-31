using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace ApexToolsLauncher.CLI.Script.Variables;

public enum EScriptVariableType
{
    Unknown = -1,
    String,
    Bool,
    Int,
    Float
}

public static class ScriptVariableTypeExtensions 
{
    public static readonly Dictionary<EScriptVariableType, string> VariableToXString = Enum.GetValues(typeof(EScriptVariableType))
        .Cast<EScriptVariableType>()
        .ToDictionary(cc => cc, cc => cc.ToString().ToLower());

    public static readonly Dictionary<string, EScriptVariableType> XStringToVariable =
        VariableToXString.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);
    
    public static EScriptVariableType ToScriptVariableType(this string str)
    {
        return XStringToVariable.GetValueOrDefault(str, EScriptVariableType.Unknown);
    }
    
    public static string AsXString(this EScriptVariableType variableType)
    {
        return VariableToXString.GetValueOrDefault(variableType, "unknown");
    }
    
    public static EScriptVariableType GetScriptVariableType(this XElement element)
    {
        var attribute = element.Attribute("type");
        if (attribute is not null)
            return attribute.Value.ToScriptVariableType();
        
        return EScriptVariableType.Unknown;;
    }
}