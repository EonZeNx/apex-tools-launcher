using System.Xml.Linq;
using RustyOptions;

namespace ApexToolsLauncher.Core.Libraries.XBuilder;

public class XProjectBuilder : XDocumentBuilder
{
    public Option<string> Type = Option<string>.None;
    public Option<string> Version = Option<string>.None;
    public Option<string> Extension = Option<string>.None;
    
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
    
    public override XDocument Build()
    {
        var root = XElementBuilder.Create("atl")
            .WithAttributeOption("type", Type)
            .WithAttributeOption("version", Version)
            .WithAttributeOption("extension", Extension)
            .Build();

        if (Root.IsSome(out var xe))
        {
            root.Add(xe);
        }
        
        return new XDocument(XDocumentLibrary.ProjectXComment(), root);
    }
}
