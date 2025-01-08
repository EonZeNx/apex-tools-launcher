using System.Xml.Linq;
using ATL.Core.Extensions;
using ATL.Core.Libraries;
using ATL.Script.Libraries;
using ATL.Script.Variables;
using RustyOptions;

namespace ATL.Script.Actions;

public class ScriptActionDelete : IScriptAction
{
    public static string NodeName { get; } = "delete";
    
    public void Process(XElement node, Dictionary<string, ScriptVariable> parentVars)
    {
        var targetAttr = node.Attribute("target");
        if (targetAttr is null)
            return;
        
        var targetPath = targetAttr.Value;
        if (targetPath.StartsWith(ScriptConstantsLibrary.VariableSymbol))
        {
            var targetVarName = targetPath.Replace(ScriptConstantsLibrary.VariableSymbol, "");
            
            var optionTargetVar = parentVars.GetValueOrNone(targetVarName);
            if (!optionTargetVar.IsSome(out var targetFromVar))
            {
                return;
            }

            var optionTarget = targetFromVar.AsString();
            if (!optionTarget.IsSome(out var targetVar))
            {
                return;
            }
            
            targetPath = targetVar;
        }
        
        if (parentVars.ContainsKey("working_directory"))
        {
            var optionWorkingDir = parentVars["working_directory"].AsString();
            if (optionWorkingDir.IsSome(out var workingDir))
            {
                targetPath = Path.Join(workingDir, targetPath);
            }
        }

        try
        {
            if (File.Exists(targetPath))
            {
                File.Delete(targetPath);
            }
            else if (Directory.Exists(targetPath))
            {
                Directory.Delete(targetPath);
            }
        }
        catch (Exception e)
        {
            ConsoleLibrary.Log(e.Message, LogType.Error);
        }
    }
}
