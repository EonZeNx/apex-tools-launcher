using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using ApexToolsLauncher.CLI.Script.Libraries;
using ApexToolsLauncher.CLI.Script.Variables;
using ApexToolsLauncher.Core.Libraries;

namespace ApexToolsLauncher.CLI.Script.Actions;

public class ScriptActionMove : IScriptAction
{
    public const string NodeName = "move";
    public string Format(string message) => $"{NodeName.ToUpper()}: {message}";
    
    public ScriptProcessResult Process(XElement node, Dictionary<string, IScriptVariable> parentVars)
    {
        var fromAttr = node.Attribute("from");
        if (fromAttr is null)
            return ScriptProcessResult.Error(Format("from attribute missing"));
        
        var toAttr = node.Attribute("to");
        if (toAttr is null)
            return ScriptProcessResult.Error(Format("to attribute missing"));

        var targetFrom = ScriptLibrary.InterpolateString(fromAttr.Value, parentVars);
        var targetTo = ScriptLibrary.InterpolateString(toAttr.Value, parentVars);
        
        var targetToDirectory = Path.GetDirectoryName(targetTo);
        if (string.IsNullOrEmpty(targetToDirectory))
            return ScriptProcessResult.Error(Format("targetToDirectory is invalid? This shouldn't happen"));

        try
        {
            if (File.Exists(targetFrom))
            {
                if (!Directory.Exists(targetToDirectory))
                    Directory.CreateDirectory(targetToDirectory);
                
                if (File.Exists(targetTo))
                    File.Delete(targetTo);

                File.Move(targetFrom, targetTo);
            }
            else if (Directory.Exists(targetFrom))
            {
                if (!Directory.Exists(targetToDirectory))
                    Directory.CreateDirectory(targetToDirectory);

                IoLibrary.CopyDirectory(targetFrom, targetTo);
            }
        }
        catch (Exception e)
        {
            return ScriptProcessResult.Error(Format($"{e.Message}"));
        }
        
        return ScriptProcessResult.Ok();
    }
}
