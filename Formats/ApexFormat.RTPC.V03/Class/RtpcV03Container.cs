﻿using System.Xml.Linq;
using ApexToolsLauncher.Core.Extensions;
using ApexToolsLauncher.Core.Hash;
using CommunityToolkit.HighPerformance;
using RustyOptions;

namespace ApexFormat.RTPC.V03.Class;

/// <summary>
/// Structure:
/// <br/>Properties - <see cref="RtpcV03Variant"/>[]
/// <br/>Containers - <see cref="RtpcV03Container"/>[]
/// <br/>AssignedPropertyCount - <see cref="uint"/>
/// </summary>
public class RtpcV03Container : RtpcV03ContainerHeader
{
    public RtpcV03Variant[] Properties = [];
    public RtpcV03Container[] Containers = [];
    public uint AssignedPropertyCount = 0;
}

public static class RtpcV03ContainerExtensions
{
    public static RtpcV03Container HeaderToContainer(this RtpcV03ContainerHeader header)
    {
        var result = new RtpcV03Container
        {
            NameHash = header.NameHash,
            Offset = header.Offset,
            PropertyCount = header.PropertyCount,
            ContainerCount = header.ContainerCount,
            Properties = new RtpcV03Variant[header.PropertyCount],
            Containers = new RtpcV03Container[header.ContainerCount],
        };

        return result;
    }
    
    public static Option<RtpcV03Container> ReadRtpcV03Container(this Stream stream)
    {
        var optionContainerHeader = stream.ReadRtpcV03ContainerHeader();
        if (!optionContainerHeader.IsSome(out var containerHeader))
            return Option<RtpcV03Container>.None;
        
        var result = containerHeader.HeaderToContainer();
        
        var originalPosition = stream.Position;
        stream.Seek(result.Offset, SeekOrigin.Begin);
        
        for (var i = 0; i < result.PropertyCount; i++)
        {
            var optionVariant = stream.ReadRtpcV03Variant();
            if (optionVariant.IsSome(out var variant))
                result.Properties[i] = variant;
        }

        stream.Align(4);
        for (var i = 0; i < result.ContainerCount; i++)
        {
            var optionContainer = stream.ReadRtpcV03Container();
            if (optionContainer.IsSome(out var container))
                result.Containers[i] = container;
        }

        result.AssignedPropertyCount = stream.Read<uint>();

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
    
    public static XElement WriteXElement(this RtpcV03Container container)
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
    
    public static void FilterBy(this RtpcV03Container container, IRtpcV03Filter[] filters)
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

    public static int CountContainers(this RtpcV03Container container)
    {
        return container.ContainerCount + container.Containers.Sum(c => c.CountContainers());
    }

    public static string PropertyString(this RtpcV03Container container, string propertyString, int depth = 0)
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