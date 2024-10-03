using System.Xml.Linq;
using ATL.Core.Libraries;
using ATL.Script.Libraries;
using RustyOptions;

namespace ATL.Script.Variables;

public class ScriptVariableAsk : IScriptVariable
{
    public const string NodeName = "ask";
    public string Format(string message) => $"{NodeName.ToUpper()}: {message}";

    public string Name { get; set; } = "UNSET";
    public EScriptVariableType Type { get; set; } = EScriptVariableType.Unknown;
    public EScriptVariableMetaType MetaType { get; set; } = EScriptVariableMetaType.Normal;
    public object? Data { get; set; } = null;
    
    public Option<T> As<T>() where T : notnull
    {
        return Data is null
            ? Option<T>.None
            : Option.Some((T) Data);
    }

    public ScriptProcessResult Process(XElement node, Dictionary<string, IScriptVariable> parentVars)
    {
        var nameAttr = node.Attribute("name");
        if (nameAttr is null)
            return ScriptProcessResult.Error(Format("name attribute missing"));

        var name = nameAttr.Value;

        var variableType = node.GetScriptVariableType();

        var message = $"Input {name} ({variableType.AsXString()}): ";
        var messageAttr = node.Attribute("message");
        if (messageAttr is not null)
        {
            message = ScriptLibrary.InterpolateString(messageAttr.Value, parentVars);
        }
        
        var userResponse = ConsoleLibrary.GetInput(message);
        if (string.IsNullOrEmpty(userResponse))
            return ScriptProcessResult.Break("user break");

        object? data = userResponse;
        switch (variableType)
        {
        case EScriptVariableType.Bool:
        {
            var success = bool.TryParse(userResponse, out var boolean);
            if (success)
                data = boolean;
            break;
        }
        case EScriptVariableType.Int:
        {
            var success = int.TryParse(userResponse, out var boolean);
            if (success)
                data = boolean;
            break;
        }
        case EScriptVariableType.Float:
        {
            var success = float.TryParse(userResponse, out var boolean);
            if (success)
                data = boolean;
            break;
        }
        case EScriptVariableType.String:
            data = ScriptLibrary.InterpolateString(userResponse, parentVars);
            break;
        case EScriptVariableType.Unknown:
        default:
            data = null;
            break;
        }
        
        Name = nameAttr.Value;
        Type = node.GetScriptVariableType();
        MetaType = node.GetScriptVariableMetaType();
        Data = data;
        
        return ScriptProcessResult.Ok();
    }
}
