using System.Xml.Linq;
using ApexFormat.ADF.V04.Enums;
using ApexToolsLauncher.Core.Class;
using ApexToolsLauncher.Core.Extensions;
using ApexToolsLauncher.Core.Libraries;
using ApexToolsLauncher.Core.Libraries.XBuilder;
using CommunityToolkit.HighPerformance;
using RustyOptions;

namespace ApexFormat.ADF.V04.Class;

/// <summary>
/// Structure:
/// <br/>NameHash - <see cref="uint"/>
/// <br/>TypeHash - <see cref="uint"/>
/// <br/>PayloadOffset - <see cref="uint"/>
/// <br/>PayloadSize - <see cref="uint"/>
/// <br/>NameIndex - <see cref="ulong"/>
/// </summary>
public class AdfV04Instance
{
    public uint NameHash      = 0;
    public uint TypeHash      = 0;
    public uint PayloadOffset = 0;
    public uint PayloadSize   = 0;
    public ulong NameIndex    = 0;

    public string Name { get; set; } = string.Empty;
}

public static class AdfV04InstanceLibrary
{
    public const uint SizeOf = sizeof(uint) // NameHash
                               + sizeof(uint) // TypeHash
                               + sizeof(uint) // PayloadOffset
                               + sizeof(uint) // PayloadSize
                               + sizeof(ulong); // NameIndex
        
    public static Option<AdfV04Instance> ReadAdfV04Instance(this Stream stream)
    {
        if (!stream.CouldRead(SizeOf))
        {
            return Option<AdfV04Instance>.None;
        }

        var result = new AdfV04Instance
        {
            NameHash = stream.Read<uint>(),
            TypeHash = stream.Read<uint>(),
            PayloadOffset = stream.Read<uint>(),
            PayloadSize = stream.Read<uint>(),
            NameIndex = stream.Read<ulong>()
        };

        return Option.Some(result);
    }

    public static Option<XElement> ToXElement(this AdfV04Instance instance, Stream stream, AdfV04Type[] types)
    {
        if (!types.FirstOrNone(t => t.TypeHash == instance.TypeHash).IsSome(out var adfType))
        {
            return Option.None<XElement>();
        }
        
        var xe = XElementBuilder.Create("instance")
            .WithAttribute("name", instance.Name.RemoveAll(XDocumentLibrary.InvalidXmlCharacters))
            .WithAttribute("type", adfType.Name.RemoveAll(XDocumentLibrary.InvalidXmlCharacters))
            .Build();

        foreach (var member in adfType.Members)
        {
            if (!types.FirstOrNone(t => t.TypeHash == instance.TypeHash).IsSome(out var memberType))
                return Option.None<XElement>();
            
            var optionXMember = instance.DataToXElement(stream, memberType, member.SafeName, types);
            if (!optionXMember.IsSome(out var xMember))
                return Option.None<XElement>();
            
            xe.Add(xMember);
        }

        return Option.Some(xe);
    }
    
    public static Option<XElement> DataToXElement(this AdfV04Instance instance, Stream stream, AdfV04Type adfType, string name, AdfV04Type[] types)
    {
        var optionXMember = Option.None<XElement>();
        switch (adfType.Type)
        {
            case EAdfV04Type.Scalar:
                optionXMember = instance.ToXScalar(stream, adfType, name);
                break;
            case EAdfV04Type.Struct:
                optionXMember = instance.ToXStruct(stream, adfType, name, types);
                break;
            case EAdfV04Type.Pointer:
                break;
            case EAdfV04Type.Array:
            case EAdfV04Type.InlineArray:
                optionXMember = instance.ToXArray(stream, adfType, name, types);
                break;
            case EAdfV04Type.String:
            case EAdfV04Type.Recursive:
            case EAdfV04Type.Bitfield:
            case EAdfV04Type.Enum:
            case EAdfV04Type.StringHash:
            case EAdfV04Type.Deferred:
                break;
            default:
                return Option.None<XElement>();
        }

        return optionXMember;
    }
    
    public static Option<XElement> ToXStruct(this AdfV04Instance instance, Stream stream, AdfV04Type adfType, string name, AdfV04Type[] types)
    {
        var xe = new XElement(adfType.Type.ToXName());
        xe.SetAttributeValue("name", name);
        xe.SetAttributeValue("type", adfType.SafeName);
        
        foreach (var member in adfType.Members)
        {
            var optionMemberType = types.FirstOrNone(t => t.TypeHash == member.TypeHash);
            if (!optionMemberType.IsSome(out var memberType))
                return Option.None<XElement>();
            
            stream.AlignRead(member.Alignment);
            
            var optionXMember = instance.DataToXElement(stream, memberType, member.SafeName, types);
            if (!optionXMember.IsSome(out var xMember))
                return Option.None<XElement>();
            
            xe.Add(xMember);
        }

        return Option.Some(xe);
    }
    
    public static Option<XElement> ToXArray(this AdfV04Instance instance, Stream stream, AdfV04Type adfType, string name, AdfV04Type[] types)
    {
        var xe = new XElement(adfType.Type.ToXName());
        xe.SetAttributeValue("name", name);
        xe.SetAttributeValue("type", adfType.SafeName);
        
        if (adfType.BitCountOrArrayLength == 0)
            return Option.Some(xe);
        
        uint arrayOffset = 0;
        uint flags = 0;
        var count = adfType.BitCountOrArrayLength;
        uint unk = 0;
        
        if (adfType.Type == EAdfV04Type.Array)
        {
            arrayOffset = stream.Read<uint>();
            flags = stream.Read<uint>();
            count = stream.Read<uint>();
            unk = stream.Read<uint>();
        }
        
        if (arrayOffset == 0 || count == 0)
            return Option.Some(xe);
        
        var optionSubType = types.FirstOrNone(t => t.TypeHash == adfType.ScalarTypeHash);
        if (!optionSubType.IsSome(out var subtype))
            return Option.Some(xe);

        var absoluteOffset = instance.PayloadOffset + arrayOffset;
        for (var i = 0; i < count; i += 1)
        {
            stream.Seek(absoluteOffset + subtype.Size * i, SeekOrigin.Begin);

            var optionXChild = subtype.Type switch
            {
                EAdfV04Type.Scalar => instance.ToXScalar(stream, subtype, name),
                EAdfV04Type.Struct => instance.ToXStruct(stream, subtype, string.Empty, types),
                _ => Option<XElement>.None
            };

            if (!optionXChild.IsSome(out var xChild))
                return Option.None<XElement>();
            
            xe.Add(xChild);
            if (subtype.Type == EAdfV04Type.Scalar && i != 0)
            {
                xe.Add(" ");
            }
        }

        return Option.Some(xe);
    }
    
    public static Option<XElement> ToXScalar(this AdfV04Instance instance, Stream stream, AdfV04Type adfType, string name)
    {
        var xe = new XElement("member");
        xe.SetAttributeValue("name", name);
        xe.SetAttributeValue("type", adfType.SafeName);
        
        switch (adfType.ScalarType)
        {
            case EAdfV04ScalarType.Signed:
                switch (adfType.Size)
                {
                    case sizeof(sbyte): xe.Add(stream.Read<sbyte>()); break;
                    case sizeof(short): xe.Add(stream.Read<short>()); break;
                    case sizeof(int): xe.Add(stream.Read<int>()); break;
                    case sizeof(long): xe.Add(stream.Read<long>()); break;
                }
                break;
            case EAdfV04ScalarType.Unsigned:
                switch (adfType.Size)
                {
                    case sizeof(byte): xe.Add(stream.Read<byte>()); break;
                    case sizeof(ushort): xe.Add(stream.Read<ushort>()); break;
                    case sizeof(uint): xe.Add(stream.Read<uint>()); break;
                    case sizeof(ulong): xe.Add(stream.Read<ulong>()); break;
                }
                break;
            case EAdfV04ScalarType.Float:
                switch (adfType.Size)
                {
                    case sizeof(float): xe.Add(stream.Read<float>()); break;
                    case sizeof(double): xe.Add(stream.Read<double>()); break;
                }
                break;
            default:
                return Option.None<XElement>();
        }

        return Option.Some(xe);
    }

    public static void TryFindName(this AdfV04Instance instance, string[] stringTable)
    {
        if ((uint) instance.NameIndex >= stringTable.Length)
            return;
        
        instance.Name = stringTable[instance.NameIndex];
    }
}