using System.Xml.Linq;
using ATL.Script.Libraries;
using RustyOptions;

namespace ATL.Script.Variables;

public class ScriptVariable : IScriptVariable
{
    public const string NodeName = "var";

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

    public void Process(XElement node, Dictionary<string, IScriptVariable> parentVars)
    {
        var xeName = node.Name.ToString();
        if (!string.Equals(xeName, NodeName))
            return ;
        
        var nameAttr = node.Attribute("name");
        if (nameAttr is null)
            return;

        var variableType = node.GetScriptVariableType();
        
        var dataAttr = node.Attribute("value");
        if (dataAttr is null)
            return;
        
        var data = dataAttr.Value;
        if (variableType is EScriptVariableType.String or EScriptVariableType.Path)
        {
            data = ScriptLibrary.InterpolateString(data, parentVars);
        }

        Name = nameAttr.Value;
        Type = node.GetScriptVariableType();
        MetaType = node.GetScriptVariableMetaType();
        Data = data;
    }
}
