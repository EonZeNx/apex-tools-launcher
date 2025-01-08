using System.Xml.Linq;
using ApexFormat.IC.V01.Enum;
using ApexToolsLauncher.Core.Class;
using ApexToolsLauncher.Core.Hash;
using CommunityToolkit.HighPerformance;
using RustyOptions;

namespace ApexFormat.IC.V01.Class;

/// <summary>
/// Structure:
/// <br/>NameHash - <see cref="uint"/>
/// <br/>Count - <see cref="byte"/>
/// <br/>Collections - <see cref="IcV01Collection"/>[]
/// </summary>
public class IcV01Container : ISizeOf
{
    public uint NameHash = 0;
    public byte Count = 0;
    public IcV01Collection[] Collections = [];

    public static uint SizeOf()
    {
        return sizeof(uint) + // NameHash
               sizeof(byte); // Count
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
            NameHash = stream.Read<uint>(),
            Count = stream.Read<byte>(),
        };
        
        result.Collections = new IcV01Collection[result.Count];
        for (var i = 0; i < result.Count; i++)
        {
            var optionContainer = stream.ReadIcV01Collection();
            if (optionContainer.IsSome(out var container))
                result.Collections[i] = container;
        }

        return Option.Some(result);
    }
    
    public static XElement ToXElement(this IcV01Container container)
    {
        var xe = new XElement("container");
        
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
}