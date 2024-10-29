using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using ATL.CLI.Script.Libraries;
using ATL.CLI.Script.Variables;
using RustyOptions;

namespace ATL.CLI.Script.Queries;

public class ScriptQuery : IScriptVariable
{
    public const string NodeName = "query";
    public string Format(string message) => $"{NodeName.ToUpper()}: {message}";
    
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

    public void AddData(IEnumerable<string> data)
    {
        if (Data is null)
        {
            Data = data.ToList();
        }
        else
        {
            var optionData = As<List<string>>();
            if (!optionData.IsSome(out var existingData))
                return;
            
            existingData.AddRange(data);
        }
    }

    public void QueryFiles(XElement node, Dictionary<string, IScriptVariable> parentVars)
    {
        var xeTarget = node.Elements("target").ToArray();
        if (xeTarget.Length == 0)
            return;

        var targets = xeTarget
            .Select(xet => ScriptLibrary.InterpolateString(xet.Value, parentVars))
            .ToArray();

        var searchPatterns = new List<string>{"*"};
        var xeSearchPatterns = node.Elements("search_pattern").ToArray();
        if (xeSearchPatterns.Length != 0)
        {
            searchPatterns = xeSearchPatterns.Select(xesp => xesp.Value).ToList();
        }
        
        var recursive = SearchOption.TopDirectoryOnly;
        var recursiveAttr = node.Attribute("recursive");
        if (recursiveAttr is not null)
        {
            if (int.TryParse(recursiveAttr.Value, out var number))
            {
                recursive = (SearchOption) Math.Clamp(number, 0, 1);
            }
        }

        foreach (var searchPattern in searchPatterns)
        {
            var files = new List<string>();

            foreach (var target in targets)
            {
                if (!Directory.Exists(target))
                    continue;
            
                files.AddRange(Directory.GetFiles(target, searchPattern, recursive));
            }

            AddData(files);
        }
    }
    
    public void QueryDirectories(XElement node, Dictionary<string, IScriptVariable> parentVars)
    {
        var xeTarget = node.Elements("target").ToArray();
        if (xeTarget.Length == 0)
            return;

        var targets = xeTarget
            .Select(xet => ScriptLibrary.InterpolateString(xet.Value, parentVars))
            .ToArray();

        var searchPatterns = new List<string>{"*"};
        var xeSearchPatterns = node.Elements("search_pattern").ToArray();
        if (xeSearchPatterns.Length != 0)
        {
            searchPatterns = xeSearchPatterns.Select(xesp => xesp.Value).ToList();
        }
        
        var recursive = SearchOption.TopDirectoryOnly;
        var xeRecursive = node.Element("recursive");
        if (xeRecursive is not null)
        {
            if (int.TryParse(xeRecursive.Value, out var number))
            {
                recursive = (SearchOption) Math.Clamp(number, 0, 1);
            }
        }

        foreach (var searchPattern in searchPatterns)
        {
            var files = new List<string>();

            foreach (var target in targets)
            {
                if (!Directory.Exists(target))
                    continue;
            
                files.AddRange(Directory.GetDirectories(target, searchPattern, recursive));
            }

            AddData(files);
        }
    }
    
    public ScriptProcessResult Process(XElement node, Dictionary<string, IScriptVariable> parentVars)
    {
        var nameAttr = node.Attribute("name");
        if (nameAttr is null)
            return ScriptProcessResult.Error(Format("name attribute missing"));

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
            case EScriptQueryType.Both:
                QueryFiles(node, parentVars);
                QueryDirectories(node, parentVars);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        
        return ScriptProcessResult.Ok();
    }
}
