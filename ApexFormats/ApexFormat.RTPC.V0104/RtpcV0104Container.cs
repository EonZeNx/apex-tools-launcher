using System.Xml.Linq;
using ATL.Core.Hash;
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
    
    public static XElement WriteXElement(this RtpcV0104Container container)
    {
        var xe = new XElement("object");

        var hashResult = LookupHashes.Get(container.NameHash);
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
        
        return xe;
    }
}