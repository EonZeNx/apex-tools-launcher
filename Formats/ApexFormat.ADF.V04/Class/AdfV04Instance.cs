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
            if (!types.FirstOrNone(t => t.TypeHash == member.TypeHash).IsSome(out var memberType))
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
                optionXMember = instance.ScalarToXElement(stream, adfType, name);
                break;
            case EAdfV04Type.Struct:
                optionXMember = instance.ToXStruct(stream, adfType, name, types);
                break;
            case EAdfV04Type.Pointer:
                break;
            case EAdfV04Type.Array:
                optionXMember = instance.ArrayToXElement(stream, adfType, name, types);
                break;
            case EAdfV04Type.InlineArray:
                optionXMember = instance.ToXInlineArray(stream, adfType, name, types);
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

    public static Option<XElement> ToXInlineArray(this AdfV04Instance instance, Stream stream, AdfV04Type adfType, string name, AdfV04Type[] types)
    {
        var xe = new XElement(adfType.Type.ToXName());
        xe.SetAttributeValue("name", name);
        xe.SetAttributeValue("type", adfType.SafeName);
        
        var optionSubType = types.FirstOrNone(t => t.TypeHash == adfType.ScalarTypeHash);
        if (!optionSubType.IsSome(out var subtype))
            return Option.Some(xe);

        for (var i = 0; i < adfType.BitCountOrArrayLength; i += 1)
        {
            if (i != 0)
            {
                xe.Add(" ");
            }
            
            var optionResult = ScalarToContent(stream, subtype);
            if (!optionResult.IsSome(out var result))
                return Option.Some(xe);
        
            xe.Add(result.ToString());
        }

        return Option.Some(xe);
    }
    
    public static Option<XElement> ArrayToXElement(this AdfV04Instance instance, Stream stream, AdfV04Type adfType, string name, AdfV04Type[] types)
    {
        var xe = new XElement(adfType.Type.ToXName());
        xe.SetAttributeValue("name", name);
        xe.SetAttributeValue("type", adfType.SafeName);
        
        var arrayOffset = stream.Read<uint>();
        var flags = stream.Read<uint>();
        var count = stream.Read<uint>();
        var unk = stream.Read<uint>();
        
        if (arrayOffset == 0 || count == 0 || instance.PayloadOffset + arrayOffset > stream.Length)
            return Option.Some(xe);
        
        var optionSubType = types.FirstOrNone(t => t.TypeHash == adfType.ScalarTypeHash);
        if (!optionSubType.IsSome(out var subtype))
            return Option.Some(xe);

        var position = stream.Position;
        var absoluteOffset = instance.PayloadOffset + arrayOffset;
        stream.Seek(absoluteOffset, SeekOrigin.Begin);
        
        for (var i = 0; i < count; i += 1)
        {
            var optionXChild = subtype.Type switch
            {
                EAdfV04Type.Scalar => instance.ScalarToXElement(stream, subtype, name),
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
        
        stream.Seek(position, SeekOrigin.Begin);

        return Option.Some(xe);
    }
    
    public static Option<XElement> ScalarToXElement(this AdfV04Instance instance, Stream stream, AdfV04Type adfType, string name)
    {
        var xe = new XElement("member");
        xe.SetAttributeValue("name", name);
        xe.SetAttributeValue("type", adfType.SafeName);

        var optionResult = ScalarToContent(stream, adfType);
        if (!optionResult.IsSome(out var result))
            return Option.None<XElement>();
        
        xe.Add(result.ToString());

        return Option.Some(xe);
    }

    public static Option<object> ScalarToContent(Stream stream, AdfV04Type adfType)
    {
        switch (adfType.ScalarType)
        {
            case EAdfV04ScalarType.Signed:
                switch (adfType.Size)
                {
                    case sizeof(sbyte): return Option.Some<object>(stream.Read<sbyte>());
                    case sizeof(short): return Option.Some<object>(stream.Read<short>());
                    case sizeof(int): return Option.Some<object>(stream.Read<int>());
                    case sizeof(long): return Option.Some<object>(stream.Read<long>());
                }
                break;
            case EAdfV04ScalarType.Unsigned:
                switch (adfType.Size)
                {
                    case sizeof(byte): return Option.Some<object>(stream.Read<byte>());
                    case sizeof(ushort): return Option.Some<object>(stream.Read<ushort>());
                    case sizeof(uint): return Option.Some<object>(stream.Read<uint>());
                    case sizeof(ulong): return Option.Some<object>(stream.Read<ulong>());
                }
                break;
            case EAdfV04ScalarType.Float:
                switch (adfType.Size)
                {
                    case sizeof(float): return Option.Some<object>(stream.Read<float>());
                    case sizeof(double): return Option.Some<object>(stream.Read<double>());
                }
                break;
            default:
                return Option.Some<object>("FAILED");
        }
        
        return Option.Some<object>("FAILED");
    }

    public static void TryFindName(this AdfV04Instance instance, string[] stringTable)
    {
        if ((uint) instance.NameIndex >= stringTable.Length)
            return;
        
        instance.Name = stringTable[instance.NameIndex];
    }
}