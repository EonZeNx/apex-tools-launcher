using System.Collections.Generic;
using ATL.CLI.Script.Actions;
using ATL.CLI.Script.Variables;

namespace ATL.CLI.Script.Blocks;

public interface IScriptBlock : IScriptAction
{
    /// <summary>
    /// Variables specific to this block only
    /// </summary>
    Dictionary<string, IScriptVariable> Variables { get; set; }
}