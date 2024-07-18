using System.Xml.Linq;
using CommunityToolkit.HighPerformance;

namespace ApexFormat.RTPC.V01;

/// <summary>
/// <para>A container and its contents are separate</para>
/// Structure:
/// <br/>NameHash - <see cref="uint"/>
/// <br/>Offset - <see cref="uint"/>
/// <br/>PropertyCount - <see cref="ushort"/>
/// <br/>ContainerCount - <see cref="ushort"/>
/// </summary>
public class RtpcV01ContainerHeader
{
    public uint NameHash = 0;
    public uint Offset = 0;
    public ushort PropertyCount = 0;
    public ushort ContainerCount = 0;

    public RtpcV01ContainerHeader() { }
}

public static class RtpcV01ContainerHeaderExtensions
{
    public static RtpcV01ContainerHeader ReadRtpcV01ContainerHeader(this Stream stream)
    {
        var result = new RtpcV01ContainerHeader
        {
            NameHash = stream.Read<uint>(),
            Offset = stream.Read<uint>(),
            PropertyCount = stream.Read<ushort>(),
            ContainerCount = stream.Read<ushort>(),
        };

        return result;
    }
    
    public static XElement WriteXElement(this RtpcV01ContainerHeader container)
    {
        var xe = new XElement("object");
        xe.SetAttributeValue("id", $"{container.NameHash:X8}");

        // foreach (var property in container.Properties)
        // {
        //     var cxe = property.WriteXElement();
        //     xe.Add(cxe);
        // }
        
        return xe;
    }
}