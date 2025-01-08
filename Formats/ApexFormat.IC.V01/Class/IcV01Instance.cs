using System.Xml.Linq;
using ApexFormat.IC.V01.Enum;
using ApexToolsLauncher.Core.Class;
using ApexToolsLauncher.Core.Extensions;
using ApexToolsLauncher.Core.Libraries;
using CommunityToolkit.HighPerformance;
using RustyOptions;

namespace ApexFormat.IC.V01.Class;

/// <summary>
/// Structure:
/// <br/>Count - <see cref="ushort"/>
/// <br/>Containers - <see cref="IcV01Collection"/>[]
/// </summary>
public class IcV01Instance
{
    public byte Count = 0;
    public IcV01Collection[] Collections = [];
    
    public byte PropertyCount = 0;
    public EIcV01CollectionType PropertyType = EIcV01CollectionType.Property;
    public string Name = string.Empty;
}

public static class IcV01InstanceLibrary
{
    public const string XName = "instance";
    public const int SizeOf = sizeof(byte); // Count
    
    public static Option<T> Read<T>(this Stream stream)
        where T : IcV01Instance
    {
        if (stream.Length - stream.Position < SizeOf)
        {
            return Option<T>.None;
        }

        var result = new IcV01Instance
        {
            Count = stream.Read<byte>(),
        };
        
        result.Collections = new IcV01Collection[result.Count];
        for (var i = 0; i < result.Count; i++)
        {
            var optionCollection = stream.Read<IcV01Collection>();
            if (optionCollection.IsSome(out var collection))
                result.Collections[i] = collection;
        }

        if (stream.Position < stream.Length)
        {
            result.PropertyCount = stream.Read<byte>();
            result.PropertyType = stream.Read<EIcV01CollectionType>();
            
            var stringLength = stream.Read<ushort>();
            result.Name = stream.ReadStringOfLength(stringLength);
        }

        return Option.Some((T) result);
    }

    public static XElement ToXElement(this IcV01Instance instance)
    {
        var xe = new XElement(XName);
        
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

    public static Result<bool, Exception> FromXElement(this IcV01Instance instance, XElement xe, bool noChildren = false)
    {
        if (string.Equals(xe.Name.LocalName, XName))
        {
            return Result.Err<bool>(new System.Xml.XmlException($"Node {xe.Name.LocalName} does not equal {XName}"));
        }
        
        xe.GetAttributeOrNone("name")
            .MatchSome(s => instance.Name = s);

        if (noChildren) return Result.OkExn(true);

        var collections = new List<IcV01Collection>(2);
        if (xe.Elements(IcV01PropertyLibrary.XName).Any())
        {
            var collection = new IcV01Collection
            {
                Type = EIcV01CollectionType.Property
            };
            var result = collection.FromXElement(xe);
            if (result.IsOk(out _))
            {
                collections.Add(collection);
            }
        }
        
        if (xe.Elements(IcV01ContainerLibrary.XName).Any())
        {
            var collection = new IcV01Collection
            {
                Type = EIcV01CollectionType.Container
            };
            var result = collection.FromXElement(xe);
            if (result.IsOk(out _))
            {
                collections.Add(collection);
            }
        }

        instance.Collections = collections.ToArray();
        instance.Count = (byte) instance.Collections.Length;

        return Result.OkExn(true);
    }

    public static Result<bool, Exception> CanRepack(this IcV01Instance instance, XElement xe)
    {
        try
        {
            var result = instance.FromXElement(xe, true);
            return result;
        }
        catch (Exception e)
        {
            return Result.Err<bool>(e);
        }
    }
}