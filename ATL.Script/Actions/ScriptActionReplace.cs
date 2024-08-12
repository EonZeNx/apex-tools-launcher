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
        
        var inFilePath = ScriptLibrary.InterpolateString(fileAttr.Value, parentVars);

        if (!File.Exists(inFilePath))
            return;
        var outFilePath = Path.Join(Path.GetDirectoryName(inFilePath), $"{Path.GetFileNameWithoutExtension(inFilePath)}_temp.txt");

        var targetAttr = node.Attribute("target");
        if (targetAttr is null)
            return;

        var target = ScriptLibrary.InterpolateString(targetAttr.Value, parentVars);

        var withAttr = node.Attribute("with");
        if (withAttr is null)
            return;

        var with = ScriptLibrary.InterpolateString(withAttr.Value, parentVars);
        
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
