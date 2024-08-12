using System.Xml.Linq;
using ATL.Core.Extensions;
using ATL.Core.Libraries;
using ATL.Script.Variables;
using RustyOptions;

namespace ATL.Script.Actions;

public class ScriptActionCopy : IScriptAction
{
    public static string NodeName { get; } = "copy";
    
    public void Process(XElement node, Dictionary<string, ScriptVariable> parentVars)
    {
        var fromAttr = node.Attribute("from");
        if (fromAttr is null)
            return;
        
        var toAttr = node.Attribute("to");
        if (toAttr is null)
            return;

        var targetFrom = fromAttr.Value;
        var targetTo = toAttr.Value;

        if (targetFrom.StartsWith(ScriptVariable.NodeSymbol))
        {
            var fromVarName = targetFrom.Replace(ScriptVariable.NodeSymbol, "");
            
            var optionFromVar = parentVars.GetValueOrNone(fromVarName);
            if (!optionFromVar.IsSome(out var targetFromVar))
            {
                return;
            }

            var optionFrom = targetFromVar.AsString();
            if (!optionFrom.IsSome(out var fromVar))
            {
                return;
            }
            
            targetFrom = fromVar;
        }
        
        if (targetTo.StartsWith(ScriptVariable.NodeSymbol))
        {
            var toVarName = targetTo.Replace(ScriptVariable.NodeSymbol, "");
            
            var optionToVar = parentVars.GetValueOrNone(toVarName);
            if (!optionToVar.IsSome(out var targetToVar))
            {
                return;
            }

            var optionTo = targetToVar.AsString();
            if (!optionTo.IsSome(out var toVar))
            {
                return;
            }
            
            targetTo = toVar;
        }

        if (parentVars.ContainsKey("working_directory"))
        {
            var optionWorkingDir = parentVars["working_directory"].AsString();
            if (optionWorkingDir.IsSome(out var workingDir))
            {
                targetFrom = Path.Join(workingDir, targetFrom);
                targetTo = Path.Join(workingDir, targetTo);
            }
        }

        try
        {
            if (File.Exists(targetFrom))
            {
                if (File.Exists(targetTo))
                    File.Delete(targetTo);
                
                File.Copy(targetFrom, targetTo);
            }
            else if (Directory.Exists(targetFrom))
            {
                if (Directory.Exists(targetTo))
                    Directory.Delete(targetTo);
                
                IOLibrary.CopyDirectory(targetFrom, targetTo);
            }
        }
        catch (Exception e)
        {
            ConsoleLibrary.Log(e.Message, LogType.Error);
        }
    }
}
