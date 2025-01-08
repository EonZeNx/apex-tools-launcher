using System.Xml.Linq;
using ApexFormat.IC.V01.Enum;
using ApexToolsLauncher.Core.Class;
using ApexToolsLauncher.Core.Extensions;
using CommunityToolkit.HighPerformance;
using RustyOptions;

namespace ApexFormat.IC.V01.Class;

/// <summary>
/// Structure:
/// <br/>Count - <see cref="ushort"/>
/// <br/>Containers - <see cref="IcV01Collection"/>[]
/// </summary>
public class IcV01Instance : ISizeOf
{
    public byte Count = 0;
    public IcV01Collection[] Collections = [];
    
    public byte PropertyCount = 0;
    public EIcV01CollectionType PropertyType = EIcV01CollectionType.Property;
    public string Name = string.Empty;

    public static uint SizeOf()
    {
        return sizeof(byte);  // Count
    }
}

public static class IcV01InstanceExtensions
{
    public static Option<IcV01Instance> ReadIcV01Instance(this Stream stream)
    {
        if (stream.Length - stream.Position < IcV01Instance.SizeOf())
        {
            return Option<IcV01Instance>.None;
        }

        var result = new IcV01Instance
        {
            Count = stream.Read<byte>(),
        };
        
        result.Collections = new IcV01Collection[result.Count];
        for (var i = 0; i < result.Count; i++)
        {
            var optionContainer = IcV01CollectionExtensions.ReadIcV01Collection(stream);
            if (optionContainer.IsSome(out var container))
                result.Collections[i] = container;
        }

        if (stream.Position < stream.Length)
        {
            result.PropertyCount = stream.Read<byte>();
            result.PropertyType = stream.Read<EIcV01CollectionType>();
            
            var stringLength = stream.Read<ushort>();
            result.Name = stream.ReadStringOfLength(stringLength);
        }

        return Option.Some(result);
    }

    public static XElement ToXElement(this IcV01Instance instance)
    {
        var xe = new XElement("instance");
        
        if (!string.IsNullOrEmpty(instance.Name))
        {
            xe.SetAttributeValue("name", instance.Name);
        }

        foreach (var collection in instance.Collections)
        {
            foreach (var cxe in collection.ToXElements())
            {
                xe.Add(cxe);
            }
        }

        return xe;
    }
}