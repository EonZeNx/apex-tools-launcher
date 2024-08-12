using ATL.Script.Actions;
using ATL.Script.Variables;

namespace ATL.Script.Blocks;

public interface IScriptBlock : IScriptAction
{
    /// <summary>
    /// Variables specific to this block only
    /// </summary>
    Dictionary<string, ScriptVariable> Variables { get; set; }
}