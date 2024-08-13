using System.Xml.Linq;
using ATL.Core.Class;
using ATL.Script.Blocks;
using ATL.Script.Variables;

namespace ATL.Script;

public class ScriptManager : IProcessBasic
{
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

        var variables = new Dictionary<string, IScriptVariable>();
        
        var scriptBlock = new ScriptBlock();
        scriptBlock.Process(xDoc, variables);
    }

    public int ProcessBasic(string inFilePath, string outDirectory)
    {
        Load(inFilePath);
        
        return 0;
    }
}