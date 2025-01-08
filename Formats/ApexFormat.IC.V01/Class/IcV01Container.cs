using System.Xml.Linq;
using ApexFormat.IC.V01.Enum;
using ApexToolsLauncher.Core.Class;
using CommunityToolkit.HighPerformance;
using RustyOptions;

namespace ApexFormat.IC.V01.Class;

/// <summary>
/// Structure:
/// <br/>Type - <see cref="EIcV01ContainerType"/>
/// <br/>Count - <see cref="ushort"/>
/// <br/>Collections OR Properties - <see cref="ushort"/>
/// </summary>
public class IcV01Container : ISizeOf
{
    public EIcV01ContainerType Type = EIcV01ContainerType.Unk0;
    public ushort Count = 0;

    public IcV01Collection[] Collections = [];
    public IcV01Property[] Properties = [];

    public static uint SizeOf()
    {
        return sizeof(EIcV01ContainerType) + // Type
               sizeof(ushort); // Count
    }
}

public static class IcV01ContainerExtensions
{
    public static Option<IcV01Container> ReadIcV01Container(this Stream stream)
    {
        if (stream.Length - stream.Position < IcV01Container.SizeOf())
        {
            return Option<IcV01Container>.None;
        }
        
        var result = new IcV01Container
        {
            Type = stream.Read<EIcV01ContainerType>(),
            Count = stream.Read<ushort>()
        };

        switch (result.Type)
        {
            case EIcV01ContainerType.Unk0:
            case EIcV01ContainerType.Collection:
            {
                result.Collections = new IcV01Collection[result.Count];
                for (var i = 0; i < result.Count; i++)
                {
                    var optionCollection = stream.ReadIcV01Collection();
                    if (optionCollection.IsSome(out var collection))
                        result.Collections[i] = collection;
                }
                break;
            }
            case EIcV01ContainerType.Property:
            {
                result.Properties = new IcV01Property[result.Count];
                for (var i = 0; i < result.Count; i++)
                {
                    var optionProperty = stream.ReadIcV01Property();
                    if (optionProperty.IsSome(out var property))
                        result.Properties[i] = property;
                }
                break;
            }
            default:
                throw new ArgumentOutOfRangeException();
        }
        
        return Option.Some(result);
    }
    
    public static XElement ToXElement(this IcV01Container container)
    {
        var xe = new XElement("container");
        xe.SetAttributeValue("type", container.Type.ToString());

        if (container.Collections.Length != 0)
        {
            foreach (var collection in container.Collections)
            {
                xe.Add(collection.ToXElement());
            }
        }
        else if (container.Properties.Length != 0)
        {
            foreach (var property in container.Properties)
            {
                xe.Add(property.ToXElement());
            }
        }

        return xe;
    }
}