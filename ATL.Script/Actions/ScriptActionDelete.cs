using System.Xml.Linq;
using ATL.Core.Extensions;
using ATL.Core.Libraries;
using ATL.Script.Libraries;
using ATL.Script.Variables;
using RustyOptions;

namespace ATL.Script.Actions;

public class ScriptActionDelete : IScriptAction
{
    public const string NodeName = "delete";
    public string Format(string message) => $"{NodeName.ToUpper()}: {message}";
    
    public ScriptProcessResult Process(XElement node, Dictionary<string, IScriptVariable> parentVars)
    {
        var targetAttr = node.Attribute("target");
        if (targetAttr is null)
            return ScriptProcessResult.Error(Format("target attribute missing"));

        var targetPath = ScriptLibrary.InterpolateString(targetAttr.Value, parentVars);

        try
        {
            if (File.Exists(targetPath))
            {
                File.Delete(targetPath);
            }
            else if (Directory.Exists(targetPath))
            {
                Directory.Delete(targetPath, true);
            }
        }
        catch (Exception e)
        {
            return ScriptProcessResult.Error(Format($"{e.Message}"));
        }
        
        return ScriptProcessResult.Ok();
    }
}
