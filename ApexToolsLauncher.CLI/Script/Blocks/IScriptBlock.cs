using System.Collections.Generic;
using ApexToolsLauncher.CLI.Script.Actions;
using ApexToolsLauncher.CLI.Script.Variables;

namespace ApexToolsLauncher.CLI.Script.Blocks;

public interface IScriptBlock : IScriptAction
{
    /// <summary>
    /// Variables specific to this block only
    /// </summary>
    Dictionary<string, IScriptVariable> Variables { get; set; }
}