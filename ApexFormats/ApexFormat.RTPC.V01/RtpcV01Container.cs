using System.Xml.Linq;
using ATL.Core.Extensions;

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

    public RtpcV01Container() : base() { }
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
    
    public static XElement WriteXElement(this RtpcV01Container container)
    {
        var xe = new XElement("object");
        xe.SetAttributeValue("id", $"{container.NameHash:X8}");
        
        foreach (var property in container.Properties)
        {
            var cxe = property.WriteXElement();
            xe.Add(cxe);
        }
        
        foreach (var childContainer in container.Containers)
        {
            var cxe = childContainer.WriteXElement();
            xe.Add(cxe);
        }
        
        return xe;
    }
}