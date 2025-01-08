using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using ATL.CLI.Script.Libraries;
using ATL.CLI.Script.Variables;

namespace ATL.CLI.Script.Actions;

public class ScriptActionProcess : IScriptAction
{
    public const string NodeName = "process";
    public string Format(string message) => $"{NodeName.ToUpper()}: {message}";
    
    public ScriptProcessResult Process(XElement node, Dictionary<string, IScriptVariable> parentVars)
    {
        var targetAttr = node.Attribute("target");
        if (targetAttr is null)
            return ScriptProcessResult.Error(Format("target attribute missing"));
        
        var target = ScriptLibrary.InterpolateString(targetAttr.Value, parentVars);
        if (!File.Exists(target))
            return ScriptProcessResult.Error(Format($"target path does not exist: '{target}'"));

        var outDirectory = Path.GetDirectoryName(target);
        if (string.IsNullOrEmpty(outDirectory))
            return ScriptProcessResult.Error(Format("outDirectory is invalid. This shouldn't happen"));
        
        var outDirectoryAttr = node.Attribute("out_directory");
        if (outDirectoryAttr is not null)
        {
            outDirectory = ScriptLibrary.InterpolateString(outDirectoryAttr.Value, parentVars);
        }
        
        try
        {
            AtlOperate.OperateFile(target, outDirectory);
        }
        catch (Exception e)
        {
            return ScriptProcessResult.Error(Format($"{e.Message}"));
        }
        
        return ScriptProcessResult.Ok();
    }
}
