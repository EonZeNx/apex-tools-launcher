using System.Xml.Linq;
using ApexToolsLauncher.Core.Extensions;
using ApexToolsLauncher.Core.Hash;
using ApexToolsLauncher.Core.Libraries;
using CommunityToolkit.HighPerformance;
using RustyOptions;

namespace ApexFormat.RTPC.V01.Class;

/// <summary>
/// Structure:
/// <br/>NameHash - <see cref="uint"/>
/// <br/>Offset - <see cref="uint"/>
/// <br/>PropertyCount - <see cref="ushort"/>
/// <br/>ContainerCount - <see cref="ushort"/>
/// <br/>Properties - <see cref="RtpcV01Property"/>[]
/// <br/>Containers - <see cref="RtpcV01Container"/>[]
/// </summary>
public class RtpcV01Container
{
    public uint NameHash = 0;
    public uint Offset = 0;
    public ushort PropertyCount = 0;
    public ushort ContainerCount = 0;
    public RtpcV01Property[] Properties = [];
    public RtpcV01Container[] Containers = [];

    public override string ToString()
    {
        return $"{NameHash:X08} ({PropertyCount}p / {ContainerCount}c)";
    }
}

public static class RtpcV01ContainerLibrary
{
    public const int SizeOf = sizeof(uint) // NameHash
                              + sizeof(uint) // Offset
                              + sizeof(ushort) // PropertyCount
                              + sizeof(ushort); // ContainerCount
    
    public static Option<RtpcV01Container> ReadRtpcV01Container(this Stream stream)
    {
        if (!stream.CouldRead(SizeOf))
        {
            return Option<RtpcV01Container>.None;
        }
        
        var result = new RtpcV01Container
        {
            NameHash = stream.Read<uint>(),
            Offset = stream.Read<uint>(),
            PropertyCount = stream.Read<ushort>(),
            ContainerCount = stream.Read<ushort>(),
        };
        
        result.Properties = new RtpcV01Property[result.PropertyCount];
        result.Containers = new RtpcV01Container[result.ContainerCount];
        
        var originalPosition = stream.Position;
        stream.Seek(result.Offset, SeekOrigin.Begin);
        
        for (var i = 0; i < result.PropertyCount; i++)
        {
            var optionVariant = stream.ReadRtpcV01Property();
            if (optionVariant.IsSome(out var variant))
                result.Properties[i] = variant;
        }

        stream.Align(4);
        for (var i = 0; i < result.ContainerCount; i++)
        {
            var optionContainer = stream.ReadRtpcV01Container();
            if (optionContainer.IsSome(out var container))
                result.Containers[i] = container;
        }

        stream.Seek(originalPosition, SeekOrigin.Begin);
        return Option.Some(result);
    }
    
    public static XElement ToXElement(this RtpcV01Container container)
    {
        var xe = new XElement("object");

        var optionHashResult = HashDatabases.Lookup(container.NameHash, EHashType.FilePath);
        if (optionHashResult.IsSome(out var hashResult))
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
        Array.Sort(children, XDocumentLibrary.SortNameThenId);

        foreach (var child in children)
        {
            xe.Add(child);
        }
        
        foreach (var childContainer in container.Containers)
        {
            xe.Add(childContainer.ToXElement());
        }
        
        return xe;
    }
    
    public static void FilterBy(this RtpcV01Container container, IRtpcV01Filter[] filters)
    {
        foreach (var childContainer in container.Containers)
        {
            // filter children
            FilterBy(childContainer, filters);
        }

        var primaryMatch = filters.FirstOrNone(f => f.MatchContainer(container));
        if (primaryMatch.IsSome(out var primaryFilter))
        {
            // filter properties
            var filteredProperties = container.Properties
                .Where(p => primaryFilter.MatchProperty(p, true))
                .ToArray();

            container.Properties = filteredProperties;
        }
        else
        {
            container.Properties = [];
        }
        
        container.PropertyCount = (ushort) container.Properties.Length;
        
        // filter child containers
        var filteredContainers = container.Containers
            .Where(c => c.PropertyCount != 0 || c.ContainerCount != 0)
            .ToArray();
        
        container.Containers = filteredContainers;
        container.ContainerCount = (ushort) filteredContainers.Length;
        
        
    }

    // ReSharper disable once UnusedMember.Global
    public static int CountContainers(this RtpcV01Container container)
    {
        return container.ContainerCount + container.Containers.Sum(c => c.CountContainers());
    }

    // ReSharper disable once UnusedMember.Global
    public static string PropertyString(this RtpcV01Container container, string propertyString, int depth = 0)
    {
        const char indentChar = '\t';
        propertyString += $"\n{new string(indentChar, depth)}>>> Container d{depth} <<<";
        
        foreach (var property in container.Properties)
        {
            propertyString += $"\n{new string(indentChar, depth)}- {property}";
        }

        foreach (var childContainer in container.Containers)
        {
            propertyString += childContainer.PropertyString(propertyString, depth + 1);
        }

        return propertyString;
    }
}