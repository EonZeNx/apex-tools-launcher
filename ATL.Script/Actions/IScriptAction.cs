using System.Xml.Linq;
using ATL.Script.Variables;

namespace ATL.Script.Actions;

public interface IScriptAction
{
    /// <summary>
    /// An action to perform.
    /// </summary>
    /// <param name="node">The XML node this action represents</param>
    /// <param name="parentVars">All parent variables</param>
    void Process(XElement node, Dictionary<string, IScriptVariable> parentVars);
}