using System.Xml.Linq;
using ATL.Core.Extensions;
using ATL.Core.Libraries;
using ATL.Script.Libraries;
using ATL.Script.Variables;
using RustyOptions;

namespace ATL.Script.Actions;

public class ScriptActionReplace : IScriptAction
{
    public static string NodeName { get; } = "replace";
    
    public void Process(XElement node, Dictionary<string, ScriptVariable> parentVars)
    {
        var fileAttr = node.Attribute("file");
        if (fileAttr is null)
            return;
        
        var inFilePath = fileAttr.Value;
        if (inFilePath.StartsWith(ScriptConstantsLibrary.VariableSymbol))
        {
            var inFilePathVarName = inFilePath.Replace(ScriptConstantsLibrary.VariableSymbol, "");
            
            var optioninFilePathVar = parentVars.GetValueOrNone(inFilePathVarName);
            if (!optioninFilePathVar.IsSome(out var inFilePathVar))
            {
                return;
            }

            var optionInFilePath = inFilePathVar.AsString();
            if (!optionInFilePath.IsSome(out var safeInFilePath))
            {
                return;
            }
            
            inFilePath = safeInFilePath;
        }

        if (!File.Exists(inFilePath))
            return;
        var outFilePath = Path.Join(Path.GetDirectoryName(inFilePath), $"{Path.GetFileNameWithoutExtension(inFilePath)}_temp.txt");

        var targetAttr = node.Attribute("target");
        if (targetAttr is null)
            return;

        var target = targetAttr.Value;

        var withAttr = node.Attribute("with");
        if (withAttr is null)
            return;

        var with = withAttr.Value;
        
        try
        {
            var lines = File.ReadLines(inFilePath);
            using (var outFile = File.CreateText(outFilePath))
            {
                foreach (var line in lines)
                {
                    var lineReady = line;

                    if (lineReady.Contains(target))
                        lineReady = lineReady.Replace(target, with);

                    outFile.WriteLine(lineReady);
                }
            }
            
            File.Delete(inFilePath);
            File.Move(outFilePath, inFilePath);
        }
        catch (Exception e)
        {
            ConsoleLibrary.Log(e.Message, LogType.Error);
        }
    }
}
