using System.Xml.Linq;

namespace ATL.Script.Variables;

public enum EScriptVariableType
{
    Unknown = -1,
    String, Path,
    Int,
    Float,
    File
}

public static class ScriptVariableTypeExtensions 
{
    public static readonly Dictionary<EScriptVariableType, string> VariableToXString = new()
    {
        { EScriptVariableType.Unknown, "unknown" },
        { EScriptVariableType.String,  "string" },
        { EScriptVariableType.Path,    "path" },
        { EScriptVariableType.Int,     "int" },
        { EScriptVariableType.Float,   "float" },
        { EScriptVariableType.File,    "file" },
    };

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