using System.Xml.Linq;
using ATL.Script.Libraries;
using ATL.Script.Variables;

namespace ATL.Script.Actions;

public interface IScriptAction
{
    /// <summary>
    /// An action to perform.
    /// </summary>
    /// <param name="node">The XML node this action represents</param>
    /// <param name="parentVars">All parent variables</param>
    ScriptProcessResult Process(XElement node, Dictionary<string, IScriptVariable> parentVars);
    
    /// <summary>
    /// Format a message for display purposes
    /// </summary>
    /// <param name="message">The message to format</param>
    /// <returns>A manipulated message</returns>
    string Format(string message);
}