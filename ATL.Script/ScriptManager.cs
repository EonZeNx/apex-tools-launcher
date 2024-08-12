using System.Xml.Linq;
using ATL.Core.Class;
using ATL.Script.Blocks;
using ATL.Script.Libraries;
using ATL.Script.Variables;

namespace ATL.Script;

public class ScriptManager : IProcessBasic
{
    protected Dictionary<string, ScriptVariable> LoadSettings(XElement element)
    {
        var result = new Dictionary<string, ScriptVariable>();

        var xeSettings = element.Element(ScriptConstantsLibrary.SettingsXString);
        if (xeSettings is null)
            return result;

        foreach (var xeSetting in xeSettings.Elements(ScriptConstantsLibrary.SettingXString))
        {
            var optionScriptVariable = xeSetting.GetScriptVariable();
            if (!optionScriptVariable.IsSome(out var scriptVariable))
                continue;

            result.TryAdd(scriptVariable.Name, scriptVariable);
        }

        return result;
    }
    
    public void Load(string filepath)
    {
        if (!Path.Exists(filepath))
        {
            return;
        }

        var xDoc = XElement.Load(filepath);
        if (!xDoc.HasElements)
        {
            return;
        }

        var variables = LoadSettings(xDoc);
        
        var scriptBlock = new ScriptBlock();
        scriptBlock.Process(xDoc, variables);
    }

    public int ProcessBasic(string inFilePath, string outDirectory)
    {
        Load(inFilePath);
        
        return 0;
    }
}