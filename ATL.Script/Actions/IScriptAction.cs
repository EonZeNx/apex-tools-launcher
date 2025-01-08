using System.Xml.Linq;
using ATL.Script.Variables;

namespace ATL.Script.Actions;

public interface IScriptAction : IScriptNode
{
    void Process(XElement element, Dictionary<string, ScriptVariable> variables);
}