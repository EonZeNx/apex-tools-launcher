using System.Xml.Linq;
using ATL.Core.Extensions;
using ATL.Core.Libraries;
using ATL.Script.Libraries;
using ATL.Script.Variables;
using RustyOptions;

namespace ATL.Script.Actions;

public class ScriptActionMove : IScriptAction
{
    public static string NodeName { get; } = "move";
    
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

        if (targetFrom.StartsWith(ScriptConstantsLibrary.VariableSymbol))
        {
            var fromVarName = targetFrom.Replace(ScriptConstantsLibrary.VariableSymbol, "");
            
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
        
        if (targetTo.StartsWith(ScriptConstantsLibrary.VariableSymbol))
        {
            var toVarName = targetTo.Replace(ScriptConstantsLibrary.VariableSymbol, "");
            
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
                
                File.Move(targetFrom, targetTo);
            }
            else if (Directory.Exists(targetFrom))
            {
                if (Directory.Exists(targetTo))
                    Directory.Delete(targetTo);
                
                Directory.Move(targetFrom, targetTo);
            }
        }
        catch (Exception e)
        {
            ConsoleLibrary.Log(e.Message, LogType.Error);
        }
    }
}
