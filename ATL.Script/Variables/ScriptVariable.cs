using System.Xml.Linq;
using ATL.Script.Libraries;
using RustyOptions;

namespace ATL.Script.Variables;

public class ScriptVariable : IScriptNode
{
    public static string NodeName { get; } = "var";
    public static string NodeSymbol { get; } = "$";
    
    public string Name { get; set; } = "UNSET";
    public EScriptVariableType VariableType { get; set; } = EScriptVariableType.Unknown;
    public object? Data { get; set; } = null;

    public Option<string> AsString()
    {
        if (Data is null)
            return Option<string>.None;
        
        return Option.Some((string) Data);
    }

    public Option<int> AsInt()
    {
        if (Data is null)
            return Option<int>.None;
        
        return Option.Some((int) Data);
    }

    public Option<float> AsFloat()
    {
        if (Data is null)
            return Option<float>.None;
        
        return Option.Some((float) Data);
    }
}

public static class ScriptVariableExtensions
{
    public static Option<ScriptVariable> GetScriptVariable(this XElement element)
    {
        var xeName = element.Name.ToString();
        if (xeName != ScriptVariable.NodeName && xeName != ScriptConstantsLibrary.SettingXString)
            return Option<ScriptVariable>.None;
        
        var nameAttr = element.Attribute("name");
        if (nameAttr is null)
            return Option<ScriptVariable>.None;
        
        var result = new ScriptVariable
        {
            Name = nameAttr.Value,
            VariableType = element.GetScriptVariableType(),
            Data = element.Attribute("value")?.Value
        };

        return Option.Some(result);
    }
}
