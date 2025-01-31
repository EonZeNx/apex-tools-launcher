using System.Xml.Linq;
using RustyOptions;

namespace ApexToolsLauncher.Core.Libraries.XBuilder;

public class XElementBuilder(string name)
{
    public XElement Element = new(name);
    
    public static XElementBuilder Create(string name) => new(name);

    public XElementBuilder WithAttribute(string name, string value)
    {
        Element.SetAttributeValue(name, value);
        return this;
    }

    public XElementBuilder WithAttribute(string name, Option<string> option)
    {
        if (option.IsSome(out var value))
        {
            Element.SetAttributeValue(name, value);
        }
        
        return this;
    }

    public XElementBuilder WithAttribute(string name, Func<Option<string>> condition)
    {
        WithAttribute(name, condition.Invoke());
        
        return this;
    }

    public XElementBuilder WithContent(string content)
    {
        Element.Value = content;
        return this;
    }

    public XElementBuilder WithChild(XElement child)
    {
        Element.Add(child);
        return this;
    }

    public XElementBuilder WithChild(Option<XElement> child)
    {
        if (child.IsSome(out var value))
        {
            Element.Add(value);
        }
        
        return this;
    }

    public XElementBuilder WithChildren(IEnumerable<XElement> children)
    {
        foreach (var child in children)
            WithChild(child);
        
        return this;
    }

    public XElementBuilder WithChildren(IEnumerable<Option<XElement>> children)
    {
        foreach (var child in children)
            WithChild(child);
        
        return this;
    }

    public XElementBuilder WithChildren<T>(IEnumerable<T> children, Func<T, XElement> func)
    {
        foreach (var child in children)
            WithChild(func(child));
        
        return this;
    }

    public XElementBuilder WithChildren<T>(IEnumerable<T> children, Func<T, Option<XElement>> func)
    {
        foreach (var child in children)
            WithChild(func(child));
        
        return this;
    }

    public XElement Build()
    {
        return Element;
    }

    public Option<XElement> BuildOption()
    {
        return Option.Create(Build());
    }
}
