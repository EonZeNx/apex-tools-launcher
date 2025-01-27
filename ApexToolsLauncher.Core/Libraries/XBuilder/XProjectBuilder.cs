using System.Xml.Linq;
using RustyOptions;

namespace ApexToolsLauncher.Core.Libraries.XBuilder;

public class XProjectBuilder
{
    public Option<string> Type = Option<string>.None;
    public Option<string> Version = Option<string>.None;
    public Option<string> Extension = Option<string>.None;
    public List<XElement> Children = [];
    
    public static XProjectBuilder CreateXProjectBuilder() => new();
    
    public virtual XProjectBuilder WithType(string type)
    {
        Type = type.AsOption();
        return this;
    }
    
    public virtual XProjectBuilder WithVersion(string version)
    {
        Version = version.AsOption();
        return this;
    }
    
    public virtual XProjectBuilder WithExtension(string extension)
    {
        Extension = extension.AsOption();
        return this;
    }

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
}
