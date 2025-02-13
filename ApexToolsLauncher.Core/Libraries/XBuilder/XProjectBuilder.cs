using System.Xml.Linq;
using RustyOptions;

namespace ApexToolsLauncher.Core.Libraries.XBuilder;

public class XProjectBuilder
{
    public Option<string> Type { get; private set; } = Option<string>.None;
    public Option<string> Version { get; private set; } = Option<string>.None;
    public Option<string> Extension { get; private set; } = Option<string>.None;
    public List<XElement> Children { get; private set; } = [];
    
    public static XProjectBuilder Create() => new();
    
    public XProjectBuilder WithType(Option<string> type)
    {
        Type = type;
        return this;
    }
    
    public XProjectBuilder WithType(string type) => WithType(type.AsOption());
    
    public XProjectBuilder WithVersion(Option<string> version)
    {
        Version = version;
        return this;
    }
    public XProjectBuilder WithVersion(string version) => WithVersion(version.AsOption());
    
    public XProjectBuilder WithExtension(Option<string> extension)
    {
        Extension = extension;
        return this;
    }
    
    public XProjectBuilder WithExtension(string extension) => WithExtension(extension.AsOption());

    public XProjectBuilder WithChild(XElement child)
    {
        Children.Add(child);
        return this;
    }

    public XProjectBuilder WithChild(Option<XElement> child)
    {
        if (child.IsSome(out var value))
        {
            Children.Add(value);
        }
        
        return this;
    }

    public XProjectBuilder WithChildren(IEnumerable<XElement> children)
    {
        foreach (var child in children)
            WithChild(child);
        
        return this;
    }

    public XProjectBuilder WithChildren(IEnumerable<Option<XElement>> children)
    {
        foreach (var child in children)
            WithChild(child);
        
        return this;
    }

    public XProjectBuilder WithChildren<T>(IEnumerable<T> children, Func<T, XElement> func)
    {
        foreach (var child in children)
            WithChild(func(child));
        
        return this;
    }

    public XProjectBuilder WithChildren<T>(IEnumerable<T> children, Func<T, Option<XElement>> func)
    {
        foreach (var child in children)
            WithChild(func(child));
        
        return this;
    }
    
    public XDocument Build()
    {
        var root = XElementBuilder.Create("atl")
            .WithAttribute("type", Type)
            .WithAttribute("version", Version)
            .WithAttribute("extension", Extension)
            .Build();

        foreach (var child in Children)
            root.Add(child);
        
        return new XDocument(XDocumentLibrary.ProjectXComment(), root);
    }

    public Option<XDocument> BuildOption()
    {
        return Option.Create(Build());
    }

    public static XProjectBuilder Load(XElement root)
    {
        var project = Create();
        
        if (!string.Equals(root.Name.ToString(), "atl"))
            return project;

        return project
            .WithType(root.GetAttribute("type"))
            .WithVersion(root.GetAttribute("version"))
            .WithExtension(root.GetAttribute("extension"));
    }

    public static XProjectBuilder Load(string path) => Load(XElement.Load(path));
}
