using System.Xml.Linq;

namespace ATL.Core.Extensions;

public static class ConsoleColorExtensions
{
    public static readonly Dictionary<ConsoleColor, string> VariableToXString = Enum.GetValues(typeof(ConsoleColor))
        .Cast<ConsoleColor>()
        .ToDictionary(cc => cc, cc => cc.ToString().ToLower());

    public static readonly Dictionary<string, ConsoleColor> XStringToVariable =
        VariableToXString.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);
    
    public static ConsoleColor ToConsoleColor(this string str)
    {
        return XStringToVariable.GetValueOrDefault(str, ConsoleColor.White);
    }
    
    public static string AsXString(this ConsoleColor variableType)
    {
        return VariableToXString.GetValueOrDefault(variableType, "white");
    }
    
    public static ConsoleColor GetConsoleColor(this XElement element)
    {
        var attribute = element.Attribute("color");
        if (attribute is not null)
            return attribute.Value.ToConsoleColor();
        
        return ConsoleColor.White;
    }
}