using System.Xml.Linq;
using ApexFormat.RTPC.V01.Enum;
using ApexToolsLauncher.Core.Extensions;
using ApexToolsLauncher.Core.Hash;
using Microsoft.VisualBasic;
using RustyOptions;

namespace ApexFormat.RTPC.V01.Class;

/// <summary>
/// Structure:
/// <br/>Properties - <see cref="RtpcV01Variant"/>[]
/// <br/>Containers - <see cref="RtpcV01Container"/>[]
/// </summary>
public class RtpcV01Container : RtpcV01ContainerHeader
{
    public RtpcV01Variant[] Properties = [];
    public RtpcV01Container[] Containers = [];

    public override string ToString()
    {
        return $"{PropertyCount} properties, {ContainerCount} containers";
    }
}

public static class RtpcV01ContainerExtensions
{
    public static RtpcV01Container HeaderToContainer(this RtpcV01ContainerHeader header)
    {
        var result = new RtpcV01Container
        {
            NameHash = header.NameHash,
            Offset = header.Offset,
            PropertyCount = header.PropertyCount,
            ContainerCount = header.ContainerCount,
            Properties = new RtpcV01Variant[header.PropertyCount],
            Containers = new RtpcV01Container[header.ContainerCount],
        };

        return result;
    }
    
    public static Option<RtpcV01Container> ReadRtpcV01Container(this Stream stream)
    {
        var optionContainerHeader = stream.ReadRtpcV01ContainerHeader();
        if (!optionContainerHeader.IsSome(out var containerHeader))
            return Option<RtpcV01Container>.None;
        
        var result = containerHeader.HeaderToContainer();
        
        var originalPosition = stream.Position;
        stream.Seek(result.Offset, SeekOrigin.Begin);
        
        for (var i = 0; i < result.PropertyCount; i++)
        {
            var optionVariant = stream.ReadRtpcV01Variant();
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

    public static int SortNameThenId(XElement x, XElement y)
    {
        var a1 = (string?) x.Attribute("name");
        var a2 = (string?) y.Attribute("name");
        
        if (!string.IsNullOrEmpty(a1) && !string.IsNullOrEmpty(a2))
        { // a1 name, a2 name
            return string.CompareOrdinal(a1, a2);
        }
        
        if (!string.IsNullOrEmpty(a1) && string.IsNullOrEmpty(a2))
        { // a1 hash?, a2 name
            return -1;
        }
        
        if (string.IsNullOrEmpty(a1) && !string.IsNullOrEmpty(a2))
        { // a1 name, a2 hash?
            return 1;
        }
        
        a1 = (string?) x.Attribute("id");
        a2 = (string?) y.Attribute("id");
        
        if (!string.IsNullOrEmpty(a1) && !string.IsNullOrEmpty(a2))
        { // a1 hash, a2 hash
            return string.CompareOrdinal(a1, a2);
        }
        
        if (!string.IsNullOrEmpty(a1) && string.IsNullOrEmpty(a2))
        { // a1 hash, a2 null
            return -1;
        }
        
        if (string.IsNullOrEmpty(a1) && !string.IsNullOrEmpty(a2))
        { // a1 null, a2 hash
            return 1;
        }

        return 0;
    }
    
    public static XElement WriteXElement(this RtpcV01Container container)
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
        Array.Sort(children, SortNameThenId);

        foreach (var child in children)
        {
            xe.Add(child);
        }
        
        foreach (var childContainer in container.Containers)
        {
            xe.Add(childContainer.WriteXElement());
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
        
        // filter self properties
        var filteredProperties = container.Properties
            .Where(p => filters
                .Any(f => f.MatchProperty(p)))
            .ToArray();

        container.Properties = filteredProperties;
        container.PropertyCount = (ushort) filteredProperties.Length;

        // filter children
        var filteredContainers = container.Containers
            .Where(c => filters
                .Any(f => f.MatchContainer(c)))
            .ToArray();
        
        container.Containers = filteredContainers.ToArray();
        container.ContainerCount = (ushort) filteredContainers.Length;
    }

    public static int CountContainers(this RtpcV01Container container)
    {
        return container.ContainerCount + container.Containers.Sum(c => c.CountContainers());
    }

    public static string PropertyString(this RtpcV01Container container, string propertyString, int depth = 0)
    {
        foreach (var property in container.Properties)
        {
            propertyString += $"\n{new string('\t', depth)}- {property}";
        }

        for (var i = 0; i < container.Containers.Length; i++)
        {
            var childContainer = container.Containers[i];
            propertyString += childContainer.PropertyString(propertyString, depth + 1);
        }

        return propertyString;
    }
}