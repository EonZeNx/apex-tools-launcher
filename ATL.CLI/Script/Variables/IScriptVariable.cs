using ATL.CLI.Script.Actions;
using RustyOptions;

namespace ATL.CLI.Script.Variables;

public interface IScriptVariable : IScriptAction
{
    string Name { get; set; }
    public EScriptVariableType Type { get; set; }
    public EScriptVariableMetaType MetaType { get; set; }
    
    /// <summary>
    /// Data for this variable. Null means the variable hasn't been set
    /// </summary>
    public object? Data { get; set; }

    public Option<T> As<T>() where T : notnull;
}