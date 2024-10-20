using System.Xml.Linq;

namespace ATL.Script.Queries;

public enum EScriptQueryType
{
    Unknown = -1,
    Files,
    Directories,
    Both
}

public static class EScriptQueryTypeExtensions 
{
    public static readonly Dictionary<EScriptQueryType, string> VariableToXString = Enum.GetValues(typeof(EScriptQueryType))
        .Cast<EScriptQueryType>()
        .ToDictionary(cc => cc, cc => cc.ToString().ToLower());

    public static readonly Dictionary<string, EScriptQueryType> XStringToVariable =
        VariableToXString.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);
    
    public static EScriptQueryType ToScriptQueryType(this string str)
    {
        return XStringToVariable.GetValueOrDefault(str, EScriptQueryType.Unknown);
    }
    
    public static string AsXString(this EScriptQueryType variableType)
    {
        return VariableToXString.GetValueOrDefault(variableType, "unknown");
    }
    
    public static EScriptQueryType GetScriptQueryType(this XElement element)
    {
        var attribute = element.Attribute("type");
        if (attribute is not null)
            return attribute.Value.ToScriptQueryType();
        
        return EScriptQueryType.Unknown;;
    }
}