using System.Xml.Linq;
using ATL.Core.Extensions;
using ATL.Core.Libraries;
using ATL.Script.Libraries;
using ATL.Script.Variables;

namespace ATL.Script.Actions;

public class ScriptActionCopy : IScriptAction
{
    public const string NodeName = "copy";
    
    public void Process(XElement node, Dictionary<string, IScriptVariable> parentVars)
    {
        var fromAttr = node.Attribute("from");
        if (fromAttr is null)
            return;
        
        var toAttr = node.Attribute("to");
        if (toAttr is null)
            return;

        var targetFrom = ScriptLibrary.InterpolateString(fromAttr.Value, parentVars);
        var targetTo = ScriptLibrary.InterpolateString(toAttr.Value, parentVars);

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
