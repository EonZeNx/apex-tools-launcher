using System.Xml.Linq;
using ATL.Console;
using ATL.Core.Libraries;
using ATL.Script.Libraries;
using ATL.Script.Variables;

namespace ATL.Script.Actions;

public class ScriptActionProcess : IScriptAction
{
    public static string NodeName { get; } = "process";
    
    public void Process(XElement node, Dictionary<string, ScriptVariable> parentVars)
    {
        var targetAttr = node.Attribute("target");
        if (targetAttr is null)
            return;
        
        var outDirectoryAttr = node.Attribute("out_directory");
        if (outDirectoryAttr is null)
            return;

        var target = ScriptLibrary.InterpolateString(targetAttr.Value, parentVars);
        var outDirectory = ScriptLibrary.InterpolateString(outDirectoryAttr.Value, parentVars);

        if (!File.Exists(target))
            return;
        
        try
        {
            AtlOperate.OperateFile(target, outDirectory);
        }
        catch (Exception e)
        {
            ConsoleLibrary.Log(e.Message, LogType.Error);
        }
    }
}
