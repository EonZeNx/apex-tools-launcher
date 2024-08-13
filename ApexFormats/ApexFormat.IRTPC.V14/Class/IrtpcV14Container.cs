using System.Xml.Linq;
using ATL.Core.Hash;
using RustyOptions;

namespace ApexFormat.IRTPC.V14.Class;

/// <summary>
/// Structure:
/// <br/>PropertyCount - <see cref="ushort"/>
/// </summary>
public class IrtpcV14Container : IrtpcV14ContainerHeader
{
    public IrtpcV14Variant[] Properties = [];
}

public static class IrtpcV14ContainerExtensions
{
    public static IrtpcV14Container HeaderToContainer(this IrtpcV14ContainerHeader header)
    {
        var result = new IrtpcV14Container
        {
            NameHash = header.NameHash,
            MajorVersion = header.MajorVersion,
            MinorVersion = header.MinorVersion,
            PropertyCount = header.PropertyCount,
        };

        return result;
    }
    
    public static Option<IrtpcV14Container> ReadIrtpcV14Container(this Stream stream)
    {
        var optionContainerHeader = stream.ReadIrtpcV14ContainerHeader();
        if (!optionContainerHeader.IsSome(out var containerHeader))
            return Option<IrtpcV14Container>.None;
        
        var result = containerHeader.HeaderToContainer();
        
        result.Properties = new IrtpcV14Variant[result.PropertyCount];
        for (var i = 0; i < result.PropertyCount; i++)
        {
            var optionVariant = stream.ReadIrtpcV14Variant();
            if (optionVariant.IsSome(out var variant))
                result.Properties[i] = variant;
        }

        return Option.Some(result);
    }

    public static int SortNameThenId(XElement x, XElement y)
    {
        var a1 = (string?) x.Attribute("name");
        var a2 = (string?) y.Attribute("name");
        
        if (!string.IsNullOrEmpty(a1) && !string.IsNullOrEmpty(a2))
        { // a1 name, a2 name
            return string.CompareOrdinal(a1, a2);
        }
        
        if (!string.IsNullOrEmpty(a1) && string.IsNullOrEmpty(a2))
        { // a1 hash?, a2 name
            return -1;
        }
        
        if (string.IsNullOrEmpty(a1) && !string.IsNullOrEmpty(a2))
        { // a1 name, a2 hash?
            return 1;
        }
        
        a1 = (string?) x.Attribute("id");
        a2 = (string?) y.Attribute("id");
        
        if (!string.IsNullOrEmpty(a1) && !string.IsNullOrEmpty(a2))
        { // a1 hash, a2 hash
            return string.CompareOrdinal(a1, a2);
        }
        
        if (!string.IsNullOrEmpty(a1) && string.IsNullOrEmpty(a2))
        { // a1 hash, a2 null
            return -1;
        }
        
        if (string.IsNullOrEmpty(a1) && !string.IsNullOrEmpty(a2))
        { // a1 null, a2 hash
            return 1;
        }

        return 0;
    }
    
    public static XElement WriteXElement(this IrtpcV14Container container)
    {
        var xe = new XElement("object");

        var optionHashResult = HashDatabases.Lookup(container.NameHash, EHashType.FilePath);
        if (optionHashResult.IsSome(out var hashResult))
        {
            xe.SetAttributeValue("name", hashResult.Value);
        }
        else
        {
            xe.SetAttributeValue("id", $"{container.NameHash:X8}");
        }

        var children = new XElement[container.PropertyCount];
        for (var i = 0; i < container.PropertyCount; i++)
        {
            children[i] = container.Properties[i].WriteXElement();
        }
        Array.Sort(children, SortNameThenId);

        foreach (var child in children)
        {
            xe.Add(child);
        }
        
        return xe;
    }
}