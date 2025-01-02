using System.Xml.Linq;
using ApexFormat.IC.V01.Enum;
using ApexToolsLauncher.Core.Extensions;
using ApexToolsLauncher.Core.Libraries;
using CommunityToolkit.HighPerformance;
using RustyOptions;

namespace ApexFormat.IC.V01.Class;

/// <summary>
/// Structure:
/// <br/>Type - <see cref="EIcV01CollectionType"/>
/// <br/>Count - <see cref="ushort"/>
/// <br/>Containers OR Properties - <see cref="IcV01Container"/> OR <see cref="IcV01Property"/>
/// </summary>
public class IcV01Collection
{
    public EIcV01CollectionType Type = EIcV01CollectionType.Unk0;
    public ushort Count = 0;

    public IcV01Container[] Containers = [];
    public IcV01Property[] Properties = [];

    public override string ToString()
    {
        return $"{Count} {Type}";
    }
}

public static class IcV01CollectionLibrary
{
    public const string XName = "collection";
    
    public const int SizeOf = sizeof(EIcV01CollectionType) // Type
                              + sizeof(ushort); // Count
    
    public static Option<T> Read<T>(this Stream stream)
        where T : IcV01Collection
    {
        if (stream.Length - stream.Position < SizeOf)
        {
            return Option<T>.None;
        }
        
        var result = new IcV01Collection
        {
            Type = stream.Read<EIcV01CollectionType>(),
            Count = stream.Read<ushort>()
        };

        switch (result.Type)
        {
            case EIcV01CollectionType.Unk0:
            case EIcV01CollectionType.Container:
            {
                result.Containers = new IcV01Container[result.Count];
                for (var i = 0; i < result.Count; i++)
                {
                    var optionContainer = stream.Read<IcV01Container>();
                    if (optionContainer.IsSome(out var container))
                        result.Containers[i] = container;
                }
                break;
            }
            case EIcV01CollectionType.Property:
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

        return Option.Some((T) result);
    }

    public static XElement ToXElement(this IcV01Collection collection)
    {
        var xe = new XElement(XName);
        xe.SetAttributeValue("type", collection.Type.ToXmlString());

        if (collection.Containers.Length != 0)
        {
            foreach (var container in collection.Containers)
            {
                xe.Add(container.ToXElement());
            }
        }
        else if (collection.Properties.Length != 0)
        {
            foreach (var property in collection.Properties)
            {
                xe.Add(property.ToXElement());
            }
        }

        return xe;
    }
    
    public static XElement[] ToXElements(this IcV01Collection collection)
    {
        var xeList = new List<XElement>();

        if (collection.Containers.Length != 0)
        {
            xeList.AddRange(collection.Containers.Select(c => c.ToXElement()));
        }
        else if (collection.Properties.Length != 0)
        {
            xeList.AddRange(collection.Properties.Select(p => p.ToXElement()));
        }
        
        var xes = xeList.ToArray();
        Array.Sort(xes, XDocumentLibrary.SortNameThenId);

        return xes;
    }

    public static Result<bool, Exception> PropertiesFromXElement(this IcV01Collection collection, XElement xe)
    {
        collection.Type = EIcV01CollectionType.Property;
        collection.Properties = (from pxe in xe.Elements(IcV01PropertyLibrary.XName)
            let property = new IcV01Property()
            let result = property.FromXElement(pxe)
            where result.IsOk(out _)
                select property
        ).ToArray();
        collection.Count = (byte) collection.Properties.Length;
        
        return Result.OkExn(true);
    }

    public static Result<bool, Exception> ContainersFromXElement(this IcV01Collection collection, XElement xe)
    {
        collection.Type = EIcV01CollectionType.Container;
        collection.Containers = (from cxe in xe.Elements(IcV01ContainerLibrary.XName)
            let container = new IcV01Container()
            let result = container.FromXElement(cxe)
            where result.IsOk(out _)
                select container
        ).ToArray();
        collection.Count = (byte) collection.Containers.Length;
        
        return Result.OkExn(true);
    }
    
    public static Result<bool, Exception> FromXElement(this IcV01Collection collection, XElement xe)
    {
        return collection.Type switch
        {
            EIcV01CollectionType.Unk0 or EIcV01CollectionType.Container => collection.ContainersFromXElement(xe),
            EIcV01CollectionType.Property => collection.PropertiesFromXElement(xe),
            _ => Result.Err<bool>(new ArgumentOutOfRangeException())
        };
    }
}