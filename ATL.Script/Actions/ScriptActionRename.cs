﻿using System.Xml.Linq;
using ATL.Script.Libraries;
using ATL.Script.Variables;

namespace ATL.Script.Actions;

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
