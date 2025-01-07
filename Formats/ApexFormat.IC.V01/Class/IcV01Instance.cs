using System.Text;
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
        if (!stream.CouldRead(SizeOf))
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

    public static Option<Exception> Write(this Stream stream, IcV01Instance instance)
    {
        stream.Write(instance.Count);
        foreach (var collection in instance.Collections)
        {
            var writeOption = stream.Write(collection);
            if (!writeOption.IsNone)
                return writeOption;
        }

        if (instance.PropertyCount != 0)
        {
            stream.Write(instance.PropertyCount);
            stream.Write(instance.PropertyType);
            stream.Write((ushort) instance.Name.Length);
            stream.Write(Encoding.UTF8.GetBytes(instance.Name));
        }
        
        return Option<Exception>.None;
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

    public static Result<T, Exception> Read<T>(this XElement xe, bool noChildren = false)
        where T : IcV01Instance
    {
        if (!string.Equals(xe.Name.LocalName, XName))
        {
            return Result.Err<T>(new InvalidOperationException($"Node {xe.Name.LocalName} does not equal {XName}"));
        }
        
        var instance = new IcV01Instance();
        if (xe.GetAttributeOrNone("name").IsSome(out var name))
        {
            instance.PropertyCount = 1;
            instance.PropertyType = EIcV01CollectionType.Unk0;
            instance.Name = name;
        }

        if (noChildren) return Result.OkExn((T) instance);

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

        return Result.OkExn((T) instance);
    }
    
    public static Result<bool, Exception> CanRepack(this IcV01Instance instance, XElement xe)
    {
        try
        {
            var result = xe.Read<IcV01Instance>(true);
            if (result.IsErr(out var e) && e is not null)
            {
                return Result.Err<bool>(e);
            }
            
            return Result.OkExn(true);
        }
        catch (Exception e)
        {
            return Result.Err<bool>(e);
        }
    }
}