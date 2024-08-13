using System.Xml.Linq;
using ATL.Script.Libraries;
using ATL.Script.Variables;
using RustyOptions;

namespace ATL.Script.Blocks;

public class ScriptBlockFile : ScriptBlock, IScriptNode
{
    public static string NodeName { get; } = "open";
    
    public override void Process(XElement node, Dictionary<string, ScriptVariable> parentVars)
    {
        var targetAttr = node.Attribute("target");
        if (targetAttr is null)
            return;
        
        var nameAttr = node.Attribute("name");
        if (nameAttr is null)
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
        
        if (parentVars.TryGetValue("working_directory", out var workingDirVar))
        {
            var optionWorkingDir = workingDirVar.AsString();
            if (optionWorkingDir.IsSome(out var workingDir))
            {
                targetPath = Path.Join(workingDir, targetPath);
            }
        }
        
        if (!File.Exists(targetPath))
            return;

        var fileVariable = new ScriptVariable
        {
            Name = nameAttr.Value,
            VariableType = EScriptVariableType.File,
            Data = targetPath
        };
        
        Variables.TryAdd(fileVariable.Name, fileVariable);
        base.Process(node, parentVars);
    }
}