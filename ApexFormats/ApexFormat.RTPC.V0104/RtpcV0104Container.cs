using System.Xml.Linq;
using CommunityToolkit.HighPerformance;

namespace ApexFormat.RTPC.V0104;

/// <summary>
/// Structure:
/// <br/>NameHash - <see cref="uint"/>
/// <br/>Version01 - <see cref="byte"/>
/// <br/>Version02 - <see cref="ushort"/>
/// <br/>PropertyCount - <see cref="ushort"/>
/// </summary>
public class RtpcV0104Container
{
    public uint NameHash = 0;
    public byte Version01 = 0;
    public ushort Version02 = 0;
    public ushort PropertyCount = 0;
    
    public RtpcV0104Variant[] Properties = [];
}

public static class RtpcV0104ContainerExtensions
{
    public static RtpcV0104Container ReadRtpcV01Container(this Stream stream)
    {
        var result = new RtpcV0104Container
        {
            NameHash = stream.Read<uint>(),
            Version01 = stream.Read<byte>(),
            Version02 = stream.Read<ushort>(),
            PropertyCount = stream.Read<ushort>(),
        };
        
        result.Properties = new RtpcV0104Variant[result.PropertyCount];
        for (var i = 0; i < result.PropertyCount; i++)
        {
            result.Properties[i] = stream.ReadRtpcV0104Variant();
        }

        return result;
    }
    
    public static XElement WriteXElement(this RtpcV0104Container container)
    {
        var xe = new XElement("object");
        xe.SetAttributeValue("id", $"{container.NameHash:X8}");

        foreach (var property in container.Properties)
        {
            var cxe = property.WriteXElement();
            xe.Add(cxe);
        }
        
        return xe;
    }
}