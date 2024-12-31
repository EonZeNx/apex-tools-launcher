using System.Xml.Linq;

namespace ApexToolsLauncher.Core.Libraries;

public enum LogType
{
    Error,
    Warning,
    Success,
    Info,
    Debug
}

public static class LogTypeExtensions
{
    public static readonly Dictionary<LogType, string> VariableToXString = new()
    {
        { LogType.Error,      "error" },
        { LogType.Warning,    "warning" },
        { LogType.Success,    "success" },
        { LogType.Info,       "info" },
        { LogType.Debug,      "debug" },
    };

    public static readonly Dictionary<string, LogType> XStringToVariable =
        VariableToXString.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);
    
    public static LogType ToLogType(this string str)
    {
        return XStringToVariable.GetValueOrDefault(str, LogType.Info);
    }
    
    public static string AsXString(this LogType variableType)
    {
        return VariableToXString.GetValueOrDefault(variableType, "error");
    }
    
    public static LogType GetLogType(this XElement element)
    {
        var attribute = element.Attribute("type");
        if (attribute is not null)
            return attribute.Value.ToLogType();
        
        return LogType.Error;
    }
}