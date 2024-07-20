using System.Xml.Linq;
using ATL.Core.Extensions;
using ATL.Core.Hash;

namespace ApexFormat.RTPC.V01;

/// <summary>
/// Structure:
/// <br/>Properties - <see cref="RtpcV01Variant"/>[]
/// <br/>Containers - <see cref="RtpcV01Container"/>[]
/// </summary>
public class RtpcV01Container : RtpcV01ContainerHeader
{
    public RtpcV01Variant[] Properties = [];
    public RtpcV01Container[] Containers = [];
}

public static class RtpcV01ContainerExtensions
{
    public static RtpcV01Container HeaderToContainer(this RtpcV01ContainerHeader header)
    {
        var result = new RtpcV01Container
        {
            NameHash = header.NameHash,
            Offset = header.Offset,
            PropertyCount = header.PropertyCount,
            ContainerCount = header.ContainerCount,
            Properties = new RtpcV01Variant[header.PropertyCount],
            Containers = new RtpcV01Container[header.ContainerCount],
        };

        return result;
    }
    
    public static RtpcV01Container ReadRtpcV01Container(this Stream stream)
    {
        var result = stream.ReadRtpcV01ContainerHeader().HeaderToContainer();
        
        var originalPosition = stream.Position;
        stream.Seek(result.Offset, SeekOrigin.Begin);
        
        for (var i = 0; i < result.PropertyCount; i++)
        {
            result.Properties[i] = stream.ReadRtpcV01Variant();
        }

        stream.Align(4);
        for (var i = 0; i < result.ContainerCount; i++)
        {
            result.Containers[i] = stream.ReadRtpcV01Container();
        }

        stream.Seek(originalPosition, SeekOrigin.Begin);
        return result;
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
    
    public static XElement WriteXElement(this RtpcV01Container container)
    {
        var xe = new XElement("object");

        var hashResult = HashDatabase.Lookup(container.NameHash);
        if (hashResult.Valid())
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
        
        foreach (var childContainer in container.Containers)
        {
            xe.Add(childContainer.WriteXElement());
        }
        
        return xe;
    }
}