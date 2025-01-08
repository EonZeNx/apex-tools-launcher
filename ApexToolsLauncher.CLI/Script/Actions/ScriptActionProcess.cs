using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using ApexToolsLauncher.CLI.Script.Libraries;
using ApexToolsLauncher.CLI.Script.Variables;
using ApexToolsLauncher.Core.Libraries;

namespace ApexToolsLauncher.CLI.Script.Actions;

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
            var managerOption = AtlOperate.GetOperator(target);
            if (!managerOption.IsSome(out var manager))
            {
                return ScriptProcessResult.Error(Format($"File not supported {target}"));
            }
            
            var pathName = Path.GetFileName(target);
            if (string.IsNullOrEmpty(pathName))
                pathName = Path.GetDirectoryName(target);
                
            ConsoleLibrary.Log($"Processing {pathName} as {manager.GetProcessorName()}", LogType.Info);
                
            AtlOperate.RunOperator(target, manager, outDirectory);
                
            ConsoleLibrary.Log($"Finished {pathName}", LogType.Info);
        }
        catch (Exception e)
        {
            return ScriptProcessResult.Error(Format($"{e.Message}"));
        }
        
        return ScriptProcessResult.Ok();
    }
}
