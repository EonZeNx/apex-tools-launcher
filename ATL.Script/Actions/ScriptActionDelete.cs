using System.Xml.Linq;
using ATL.Core.Extensions;
using ATL.Core.Libraries;
using ATL.Script.Variables;
using RustyOptions;

namespace ATL.Script.Actions;

public class ScriptActionDelete : IScriptAction
{
    public static string NodeName { get; } = "delete";
    
    public void Process(XElement element, Dictionary<string, ScriptVariable> variables)
    {
        var targetAttr = element.Attribute("target");
        if (targetAttr is null)
            return;
        
        var targetPath = targetAttr.Value;
        if (targetPath.StartsWith(ScriptVariable.NodeSymbol))
        {
            var targetVarName = targetPath.Replace(ScriptVariable.NodeSymbol, "");
            
            var optionTargetVar = variables.GetValueOrNone(targetVarName);
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
        
        if (variables.ContainsKey("working_directory"))
        {
            var optionWorkingDir = variables["working_directory"].AsString();
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
