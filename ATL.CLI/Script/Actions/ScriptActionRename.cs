﻿using System.Collections.Generic;
using System.Xml.Linq;
using ATL.CLI.Script.Libraries;
using ATL.CLI.Script.Variables;

namespace ATL.CLI.Script.Actions;

public class ScriptActionRename : IScriptAction
{
    public const string NodeName = "rename";
    public string Format(string message) => $"{NodeName.ToUpper()}: {message}";
    
    public ScriptProcessResult Process(XElement node, Dictionary<string, IScriptVariable> parentVars)
    {
        var scriptActionMove = new ScriptActionMove();
        var result = scriptActionMove.Process(node, parentVars);
        
        return ScriptProcessResult.Ok();
    }
}
