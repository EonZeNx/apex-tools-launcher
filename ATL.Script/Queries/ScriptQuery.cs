using System.Xml.Linq;
using ATL.Script.Libraries;
using ATL.Script.Variables;
using RustyOptions;

namespace ATL.Script.Queries;

public class ScriptQuery : IScriptVariable
{
    public const string NodeName = "query";
    
    public string Name { get; set; } = "UNSET";
    public EScriptVariableType Type { get; set; } = EScriptVariableType.Unknown;
    public EScriptVariableMetaType MetaType { get; set; } = EScriptVariableMetaType.Normal;
    public object? Data { get; set; } = null;
    
    public Option<T> As<T>() where T : notnull
    {
        return Data is null
            ? Option<T>.None
            : Option.Some((T) Data);
    }

    public void QueryFiles(XElement node, Dictionary<string, IScriptVariable> parentVars)
    {
        var xeTarget = node.Elements("target").ToArray();
        if (xeTarget.Length == 0)
            return;

        var targets = xeTarget
            .Select(xet => ScriptLibrary.InterpolateString(xet.Value, parentVars))
            .ToArray();

        var searchPattern = "*";
        var xeSearchPattern = node.Element("search_pattern");
        if (xeSearchPattern is not null)
            searchPattern = xeSearchPattern.Value;
        
        var recursive = SearchOption.TopDirectoryOnly;
        var xeRecursive = node.Element("recursive");
        if (xeRecursive is not null)
        {
            if (int.TryParse(xeRecursive.Value, out var number))
            {
                recursive = (SearchOption) Math.Clamp(number, 0, 1);
            }
        }

        var files = new List<string>();

        foreach (var target in targets)
        {
            if (!Directory.Exists(target))
                continue;
            
            files.AddRange(Directory.GetFiles(target, searchPattern, recursive));
        }
        
        Data = files.ToList();
    }
    
    public void QueryDirectories(XElement node, Dictionary<string, IScriptVariable> parentVars)
    {
        var xeTarget = node.Elements("target").ToArray();
        if (xeTarget.Length == 0)
            return;

        var targets = xeTarget
            .Select(xet => ScriptLibrary.InterpolateString(xet.Value, parentVars))
            .ToArray();

        var searchPattern = "*";
        var xeSearchPattern = node.Element("search_pattern");
        if (xeSearchPattern is not null)
            searchPattern = xeSearchPattern.Value;
        
        var recursive = SearchOption.TopDirectoryOnly;
        var xeRecursive = node.Element("recursive");
        if (xeRecursive is not null)
        {
            if (int.TryParse(xeRecursive.Value, out var number))
            {
                recursive = (SearchOption) Math.Clamp(number, 0, 1);
            }
        }

        var files = new List<string>();

        foreach (var target in targets)
        {
            if (!Directory.Exists(target))
                continue;
            
            files.AddRange(Directory.GetDirectories(target, searchPattern, recursive));
        }
        
        Data = files.ToList();
    }
    
    public void Process(XElement node, Dictionary<string, IScriptVariable> parentVars)
    {
        var xeName = node.Name.ToString();
        if (!string.Equals(xeName, NodeName))
            return ;
        
        var nameAttr = node.Attribute("name");
        if (nameAttr is null)
            return;

        Name = nameAttr.Value;
        Type = EScriptVariableType.String;
        MetaType = EScriptVariableMetaType.List;
        
        var variableType = node.GetScriptQueryType();
        switch (variableType)
        {
            case EScriptQueryType.Unknown:
                break;
            case EScriptQueryType.Files:
                QueryFiles(node, parentVars);
                break;
            case EScriptQueryType.Directories:
                QueryDirectories(node, parentVars);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}
