using System.Xml.Linq;

namespace ATL.Script.Variables;

public enum EScriptVariableMetaType
{
    Unknown = -1,
    Normal,
    List
}

public static class EScriptVariableMetaTypeExtensions 
{
    public static readonly Dictionary<EScriptVariableMetaType, string> VariableToXString = new()
    {
        { EScriptVariableMetaType.Unknown, "unknown" },
        { EScriptVariableMetaType.Normal,  "normal" },
        { EScriptVariableMetaType.List,    "list" },
    };

    public static readonly Dictionary<string, EScriptVariableMetaType> XStringToVariable =
        VariableToXString.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);
    
    public static EScriptVariableMetaType ToScriptVariableMetaType(this string str)
    {
        return XStringToVariable.GetValueOrDefault(str, EScriptVariableMetaType.Normal);
    }
    
    public static string AsXString(this EScriptVariableMetaType variableType)
    {
        return VariableToXString.GetValueOrDefault(variableType, "unknown");
    }
    
    public static EScriptVariableMetaType GetScriptVariableMetaType(this XElement element)
    {
        var attribute = element.Attribute("metatype");
        if (attribute is not null)
            return attribute.Value.ToScriptVariableMetaType();
        
        return EScriptVariableMetaType.Normal;;
    }
}