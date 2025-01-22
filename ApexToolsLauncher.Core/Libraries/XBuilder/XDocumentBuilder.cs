using System.Xml.Linq;
using RustyOptions;

namespace ApexToolsLauncher.Core.Libraries.XBuilder;

public class XDocumentBuilder()
{
    public Option<XElement> Root = Option<XElement>.None;
    
    public static XDocumentBuilder CreateXDocumentBuilder() => new();

    public virtual XDocumentBuilder WithRoot(XElement xe)
    {
        Root = xe.AsOption();
        
        return this;
    }

    public virtual XDocumentBuilder WithRootOption(Option<XElement> option)
    {
        Root = option;
        
        return this;
    }

    public virtual XDocument Build()
    {
        List<XNode> children = [];

        if (Root.IsSome(out var root))
        {
            children.Add(root);
        }

        return new XDocument(children);
    }
}
