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
        var inFilePath = "EMPTY";
        var fileAttr = node.Attribute("file");
        var fileChild = node.Element("file");
        
        if (fileAttr is not null)
        {
            inFilePath = fileAttr.Value;
        }
        else if (fileChild is not null)
        {
            inFilePath = fileChild.Value;
        }
        inFilePath = ScriptLibrary.InterpolateString(inFilePath, parentVars);

        if (!File.Exists(inFilePath))
            return;
        var outFilePath = Path.Join(Path.GetDirectoryName(inFilePath), $"{Path.GetFileNameWithoutExtension(inFilePath)}_temp.txt");

        // TODO: Check child elements instead (as well?) of attributes
        // Attributes cannot have certain characters, contents have much less restrictions

        var target = "EMPTY";
        var targetAttr = node.Attribute("target");
        var targetChild = node.Element("target");
        
        if (targetAttr is not null)
        {
            target = targetAttr.Value;
        }
        else if (targetChild is not null)
        {
            target = targetChild.Value;
        }
        target = ScriptLibrary.InterpolateString(target, parentVars);
        
        var with = "EMPTY";
        var withAttr = node.Attribute("with");
        var withChild = node.Element("with");
        
        if (withAttr is not null)
        {
            with = withAttr.Value;
        }
        else if (withChild is not null)
        {
            with = withChild.Value;
        }
        with = ScriptLibrary.InterpolateString(with, parentVars);
        
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
