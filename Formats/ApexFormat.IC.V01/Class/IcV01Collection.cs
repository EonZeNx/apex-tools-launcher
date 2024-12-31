using System.Xml.Linq;
using ApexFormat.IC.V01.Enum;
using ApexToolsLauncher.Core.Class;
using ApexToolsLauncher.Core.Libraries;
using CommunityToolkit.HighPerformance;
using RustyOptions;

namespace ApexFormat.IC.V01.Class;

/// <summary>
/// Structure:
/// <br/>Type - <see cref="EIcV01CollectionType"/>
/// <br/>Count - <see cref="ushort"/>
/// <br/>Collections OR Properties - <see cref="ushort"/>
/// </summary>
public class IcV01Collection : ISizeOf
{
    public EIcV01CollectionType Type = EIcV01CollectionType.Unk0;
    public ushort Count = 0;

    public IcV01Container[] Containers = [];
    public IcV01Property[] Properties = [];

    public static uint SizeOf()
    {
        return sizeof(EIcV01CollectionType) + // Type
               sizeof(ushort); // Count
    }
}

public static class IcV01CollectionExtensions
{
    public static Option<IcV01Collection> ReadIcV01Collection(this Stream stream)
    {
        if (stream.Length - stream.Position < IcV01Collection.SizeOf())
        {
            return Option<IcV01Collection>.None;
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
                    var optionCollection = stream.ReadIcV01Container();
                    if (optionCollection.IsSome(out var collection))
                        result.Containers[i] = collection;
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
        
        return Option.Some(result);
    }
    
    public static XElement ToXElement(this IcV01Collection collection)
    {
        var xe = new XElement("collection");
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
}