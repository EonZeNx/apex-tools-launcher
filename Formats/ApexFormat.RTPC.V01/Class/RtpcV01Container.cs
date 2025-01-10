﻿using System.Globalization;
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

    public const string XName = "object";
    
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

    public static Option<Exception> Write(this Stream stream, RtpcV01Container container)
    {
        try
        {
            stream.Write(container.NameHash);
            stream.Write(container.Offset);
            stream.Write(container.PropertyCount);
            stream.Write(container.ContainerCount);
        }
        catch (Exception e)
        {
            return Option.Some(e);
        }
        
        return Option<Exception>.None;
    }
    
    public static Option<Exception> WriteData(this Stream stream, RtpcV01Container container, Dictionary<string, uint> stringMap)
    {
        container.Offset = (uint) stream.Position;
        var containerHeaderOffset = ByteExtensions.Align(container.Offset + container.PropertyCount * RtpcV01PropertyLibrary.SizeOf, 4);
        var dataOffset = containerHeaderOffset + container.ContainerCount * SizeOf;
        
        if (!container.Properties.Empty())
        { // property data
            stream.Seek(dataOffset, SeekOrigin.Begin);
            
            foreach (var property in container.Properties)
            {
                stream.WriteData(property, stringMap);
            }
            
            stream.Align(4);
            dataOffset = stream.Position;
            
            stream.Seek(container.Offset, SeekOrigin.Begin);
            
            foreach (var property in container.Properties)
            {
                stream.Write(property);
            }
            
            stream.Align(4);
        }
        
        if (!container.Containers.Empty())
        { // container data
            stream.Seek(dataOffset, SeekOrigin.Begin);
            
            foreach (var childContainer in container.Containers)
            {
                stream.WriteData(childContainer, stringMap);
            }
            
            dataOffset = stream.Position;
            
            stream.Seek(containerHeaderOffset, SeekOrigin.Begin);
            stream.Align(4);
            
            foreach (var childContainer in container.Containers)
            {
                stream.Write(childContainer);
            }
        }
        
        stream.Seek(dataOffset, SeekOrigin.Begin);
        
        return Option<Exception>.None;
    }
    
    public static Result<RtpcV01Container, Exception> ToRtpcV01Container(this XElement xe)
    {
        if (!string.Equals(xe.Name.LocalName, XName))
        {
            return Result.Err<RtpcV01Container>(new InvalidOperationException($"Node {xe.Name.LocalName} does not equal {XName}"));
        }
        
        var nameHashOption = xe.GetAttributeOrNone("name")
            .Map(s => s.Jenkins())
            .OrElse(() => xe.GetAttributeOrNone("id")
                .Map(s => uint.Parse(s, NumberStyles.HexNumber)));
        
        if (!nameHashOption.IsSome(out var nameHash))
        {
            return Result.Err<RtpcV01Container>(new InvalidOperationException("both name and id attributes are both missing"));
        }

        var container = new RtpcV01Container
        {
            NameHash = nameHash,
            Properties = (from pxe in xe.Elements(RtpcV01PropertyLibrary.XName)
                    let property = new RtpcV01Property()
                    let result = property.FromXElement(pxe)
                    where result.IsOk(out _)
                    select property
                ).ToArray(),
            Containers = (from cxe in xe.Elements(XName)
                    let result = cxe.ToRtpcV01Container()
                    where result.IsOk(out _)
                    select result.Unwrap()
                ).ToArray()
        };

        container.PropertyCount = (ushort) container.Properties.Length;
        container.ContainerCount = (ushort) container.Containers.Length;

        return Result.OkExn(container);
    }
    
    public static XElement ToXElement(this RtpcV01Container container)
    {
        var xe = new XElement(XName);

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
            children[i] = container.Properties[i].ToXElement();
        }
        // Array.Sort(children, XDocumentLibrary.SortNameThenId);

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
    
    public static Result<bool, Exception> CanRepack(this RtpcV01Container container, XElement xe)
    {
        try
        {
            var result = xe.ToRtpcV01Container();

            if (result.Err().IsSome(out var e))
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