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
/// <br/>Containers - <see cref="IcV01Container"/>[]
/// </summary>
public class IcV01Instance : ISizeOf
{
    public byte Count = 0;
    public IcV01Container[] Containers = [];
    
    public byte PropertyCount = 0;
    public EIcV01ContainerType PropertyType = EIcV01ContainerType.Property;
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
        
        result.Containers = new IcV01Container[result.Count];
        for (var i = 0; i < result.Count; i++)
        {
            var optionContainer = stream.ReadIcV01Container();
            if (optionContainer.IsSome(out var container))
                result.Containers[i] = container;
        }

        if (stream.Position < stream.Length)
        {
            result.PropertyCount = stream.Read<byte>();
            result.PropertyType = stream.Read<EIcV01ContainerType>();
            
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

        foreach (var container in instance.Containers)
        {
            xe.Add(container.ToXElement());
        }

        return xe;
    }
}