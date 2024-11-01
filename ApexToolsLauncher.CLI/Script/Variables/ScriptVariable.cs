using System.Collections.Generic;
using System.Xml.Linq;
using ApexToolsLauncher.CLI.Script.Libraries;
using RustyOptions;

namespace ApexToolsLauncher.CLI.Script.Variables;

public class ScriptVariable : IScriptVariable
{
    public const string NodeName = "var";
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

        var variableType = node.GetScriptVariableType();
        
        var dataAttr = node.Attribute("value");
        if (dataAttr is null)
            return ScriptProcessResult.Error(Format("value attribute missing"));
        
        var data = dataAttr.Value;
        if (variableType is EScriptVariableType.String)
        {
            data = ScriptLibrary.InterpolateString(data, parentVars);
        }

        Name = nameAttr.Value;
        Type = node.GetScriptVariableType();
        MetaType = node.GetScriptVariableMetaType();
        Data = data;
        
        return ScriptProcessResult.Ok();
    }
}
