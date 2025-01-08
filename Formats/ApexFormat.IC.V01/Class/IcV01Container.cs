using System.Globalization;
using System.Xml.Linq;
using ApexFormat.IC.V01.Enum;
using ApexToolsLauncher.Core.Extensions;
using ApexToolsLauncher.Core.Hash;
using ApexToolsLauncher.Core.Libraries;
using CommunityToolkit.HighPerformance;
using RustyOptions;

namespace ApexFormat.IC.V01.Class;

/// <include file='..\docs.ICv01.xml' path='doc/members[@name="IcV01Container"]/IcV01Container/*'/>
public class IcV01Container
{
    /// <include file='..\docs.ICv01.xml' path='doc/members[@name="IcV01Container"]/NameHash/*'/>
    public uint NameHash = 0;
    /// <include file='..\docs.ICv01.xml' path='doc/members[@name="IcV01Container"]/Count/*'/>
    public byte Count = 0;
    /// <include file='..\docs.ICv01.xml' path='doc/members[@name="IcV01Container"]/Collections/*'/>
    public IcV01Collection[] Collections = [];
}

/// <include file='..\docs.ICv01.xml' path='doc/members[@name="IcV01ContainerLibrary"]/IcV01ContainerLibrary/*'/>
public static class IcV01ContainerLibrary
{
    public const string XName = "object";
    
    /// <include file='..\docs.ICv01.xml' path='doc/members[@name="IcV01ContainerLibrary"]/SizeOf/*'/>
    public const int SizeOf = sizeof(uint) // NameHash
                              + sizeof(byte); // Count

    /// <include file='..\docs.ICv01.xml' path='doc/members[@name="IcV01ContainerLibrary"]/Read/*'/>
    public static Option<T> Read<T>(this Stream stream)
        where T : IcV01Container
    {
        if (stream.Length - stream.Position < SizeOf)
        {
            return Option<T>.None;
        }

        var result = new IcV01Container
        {
            NameHash = stream.Read<uint>(),
            Count = stream.Read<byte>(),
        };
        
        result.Collections = new IcV01Collection[result.Count];
        for (var i = 0; i < result.Count; i++)
        {
            var optionCollection = stream.Read<IcV01Collection>();
            if (optionCollection.IsSome(out var collection))
                result.Collections[i] = collection;
        }

        return Option.Some((T) result);
    }
    
    /// <include file='..\docs.ICv01.xml' path='doc/members[@name="IcV01ContainerLibrary"]/ToXElement/*'/>
    public static XElement ToXElement(this IcV01Container container)
    {
        var xe = new XElement(XName);
        
        var optionHashResult = HashDatabases.Lookup(container.NameHash);
        if (optionHashResult.IsSome(out var hashResult))
        {
            xe.SetAttributeValue("name", hashResult.Value);
        }
        else
        {
            xe.SetAttributeValue("id", $"{container.NameHash:X8}");
        }

        Array.Sort(container.Collections, (a, b) => a.Type == EIcV01CollectionType.Property ? -1 : 1);
        foreach (var collection in container.Collections)
        {
            foreach (var cxe in collection.ToXElements())
            {
                xe.Add(cxe);
            }
        }

        return xe;
    }

    public static Result<bool, Exception> FromXElement(this IcV01Container container, XElement xe)
    {
        if (string.Equals(xe.Name.LocalName, XName))
        {
            return Result.Err<bool>(new System.Xml.XmlException($"Node {xe.Name.LocalName} does not equal {XName}"));
        }
        
        xe.GetAttributeOrNone("name").Match(
            s => container.NameHash = s.Jenkins(),
            () => xe.GetAttributeOrNone("id")
                .MatchSome(s => container.NameHash = uint.Parse(s, NumberStyles.HexNumber)));

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
        
        if (xe.Elements(XName).Any())
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

        container.Collections = collections.ToArray();
        container.Count = (byte) container.Collections.Length;
        
        return Result.OkExn(true);
    }
}