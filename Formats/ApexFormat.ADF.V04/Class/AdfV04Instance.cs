using System.Globalization;
using System.Text;
using System.Xml.Linq;
using ApexFormat.ADF.V04.Enums;
using ApexFormat.ADF.V04.Libraries;
using ApexToolsLauncher.Core.Extensions;
using ApexToolsLauncher.Core.Hash;
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
    
    public const string XName = "instance";
        
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

    #region To XElement
    
    public static Option<XElement> ToXElement(this AdfV04Instance instance, Stream stream, AdfV04Type[] types, Dictionary<uint, string> stringHashes)
    {
        if (!types.FirstOrNone(t => t.TypeHash == instance.TypeHash).IsSome(out var adfType))
            return Option<XElement>.None;
        
        var xe = XElementBuilder.Create("instance")
            .WithAttribute("name", instance.Name.RemoveAll(XDocumentLibrary.InvalidXmlCharacters))
            .WithAttribute("type", adfType.Name.RemoveAll(XDocumentLibrary.InvalidXmlCharacters))
            .WithAttribute("typeHash", adfType.TypeHash.ToString())
            .Build();

        foreach (var member in adfType.Members)
        {
            if (!types.FirstOrNone(t => t.TypeHash == member.TypeHash).IsSome(out var memberType))
                return Option.Create(xe);
            
            var optionXMember = instance.DataToXElement(stream, memberType, member.SafeName, types, stringHashes);
            if (!optionXMember.IsSome(out var xMember))
                return Option.Create(xe);
            
            xe.Add(xMember);
        }

        return Option.Create(xe);
    }
    
    public static Option<XElement> DataToXElement(this AdfV04Instance instance, Stream stream, AdfV04Type adfType, string name, AdfV04Type[] types, Dictionary<uint, string> stringHashes)
    {
        var optionXMember = adfType.Type switch
        {
            EAdfV04Type.Scalar =>       instance.ScalarToXElement(stream, adfType, name),
            EAdfV04Type.Struct =>       instance.StructToXElement(stream, adfType, name, types, stringHashes),
            EAdfV04Type.Pointer =>      instance.PointerToXElement(stream, adfType, name, types, stringHashes),
            EAdfV04Type.Array =>        instance.ArrayToXElement(stream, adfType, name, types, stringHashes),
            EAdfV04Type.InlineArray =>  instance.InlineArrayToXElement(stream, adfType, name, types),
            EAdfV04Type.String =>       instance.StringToXElement(stream, adfType, name),
            EAdfV04Type.Recursive =>    Option<XElement>.None,
            EAdfV04Type.Bitfield =>     instance.BitfieldToXElement(stream, adfType, name),
            EAdfV04Type.Enum =>         instance.EnumToXElement(stream, adfType, name),
            EAdfV04Type.StringHash =>   instance.StringHashToXElement(stream, adfType, name, stringHashes),
            EAdfV04Type.Deferred =>     instance.PointerToXElement(stream, adfType, name, types, stringHashes),
            _ =>                        Option<XElement>.None
        };

        return optionXMember;
    }
    
    public static Option<XElement> StructToXElement(this AdfV04Instance instance, Stream stream, AdfV04Type adfType, string name, AdfV04Type[] types, Dictionary<uint, string> stringHashes)
    {
        var xe = new XElement(adfType.Type.ToXName());
        xe.SetAttributeValue("name", name);
        xe.SetAttributeValue("type", adfType.SafeName);
        xe.SetAttributeValue("typeHash", adfType.TypeHash);
        xe.SetAttributeValue("offset", $"{stream.Position:X08}");
        
        stream.AlignRead(adfType.Alignment);
        
        foreach (var member in adfType.Members)
        {
            var optionMemberType = types.FirstOrNone(t => t.TypeHash == member.TypeHash);
            if (!optionMemberType.IsSome(out var memberType))
                return Option.Create(xe);
            
            stream.AlignRead(member.Alignment);
            
            var optionXMember = instance.DataToXElement(stream, memberType, member.SafeName, types, stringHashes);
            if (!optionXMember.IsSome(out var xMember))
                return Option.Create(xe);
            
            xe.Add(xMember);
        }
        
        stream.AlignRead(adfType.Alignment);

        return Option.Create(xe);
    }

    public static Option<XElement> InlineArrayToXElement(this AdfV04Instance instance, Stream stream, AdfV04Type adfType, string name, AdfV04Type[] types)
    {
        var xe = new XElement(adfType.Type.ToXName());
        xe.SetAttributeValue("name", name);
        xe.SetAttributeValue("type", adfType.SafeName);
        xe.SetAttributeValue("typeHash", adfType.TypeHash);
        xe.SetAttributeValue("offset", $"{stream.Position:X08}");
        
        var optionSubType = types.FirstOrNone(t => t.TypeHash == adfType.ScalarTypeHash);
        if (!optionSubType.IsSome(out var subtype))
            return Option.Create(xe);

        stream.AlignRead(adfType.Alignment);
        
        for (var i = 0; i < adfType.BitCountOrArrayLength; i += 1)
        {
            if (i != 0)
            {
                xe.Add(" ");
            }
            
            var optionResult = ScalarToContent(stream, subtype);
            if (!optionResult.IsSome(out var result))
                return Option.Create(xe);
        
            xe.Add(result.ToString());
        }

        return Option.Create(xe);
    }
    
    public static Option<XElement> ArrayToXElement(this AdfV04Instance instance, Stream stream, AdfV04Type adfType, string name, AdfV04Type[] types, Dictionary<uint, string> stringHashes)
    {
        var xe = new XElement(adfType.Type.ToXName());
        xe.SetAttributeValue("name", name);
        xe.SetAttributeValue("type", adfType.SafeName);
        xe.SetAttributeValue("typeHash", adfType.TypeHash);
        xe.SetAttributeValue("offset", $"{stream.Position:X08}");
        
        var arrayOffset = stream.Read<uint>();
        var flags = stream.Read<uint>();
        var count = stream.Read<uint>();
        var unk = stream.Read<uint>();
        
        if (arrayOffset == 0 || count == 0 || instance.PayloadOffset + arrayOffset > stream.Length)
            return Option.Create(xe);
        
        var optionSubType = types.FirstOrNone(t => t.TypeHash == adfType.ScalarTypeHash);
        if (!optionSubType.IsSome(out var subtype))
            return Option.Create(xe);

        var position = stream.Position;
        var absoluteOffset = instance.PayloadOffset + arrayOffset;
        stream.Seek(absoluteOffset, SeekOrigin.Begin);
        
        for (var i = 0; i < count; i += 1)
        {
            var optionXChild = subtype.Type switch
            {
                EAdfV04Type.Struct => instance.DataToXElement(stream, subtype, i.ToString(), types, stringHashes),
                _ => instance.DataToXElement(stream, subtype, name, types, stringHashes)
            };

            if (!optionXChild.IsSome(out var xChild))
                return Option<XElement>.None;
            
            xe.Add(xChild);
        }
        
        stream.Seek(position, SeekOrigin.Begin);

        return Option.Create(xe);
    }
    
    public static Option<XElement> ScalarToXElement(this AdfV04Instance instance, Stream stream, AdfV04Type adfType, string name)
    {
        var xe = new XElement(AdfV04MemberLibrary.XName);
        xe.SetAttributeValue("name", name);
        xe.SetAttributeValue("type", adfType.SafeName);
        xe.SetAttributeValue("typeHash", adfType.TypeHash.ToString());
        xe.SetAttributeValue("offset", $"{stream.Position:X08}");

        var optionResult = ScalarToContent(stream, adfType);
        if (!optionResult.IsSome(out var result))
            return Option<XElement>.None;
        
        xe.Add(result.ToString());

        return Option.Create(xe);
    }

    public static Option<object> ScalarToContent(Stream stream, AdfV04Type adfType)
    {
        stream.AlignRead(adfType.Alignment);
        
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

    public static Option<XElement> StringToXElement(this AdfV04Instance instance, Stream stream, AdfV04Type adfType, string name)
    {
        var xe = new XElement(AdfV04MemberLibrary.XName);
        xe.SetAttributeValue("name", name);
        xe.SetAttributeValue("type", adfType.SafeName);
        xe.SetAttributeValue("typeHash", adfType.TypeHash.ToString());
        xe.SetAttributeValue("offset", $"{stream.Position:X08}");
        
        var stringOffset = stream.Read<uint>();
        var unk0 = stream.Read<uint>();
        if (stringOffset == 0 || instance.PayloadOffset + stringOffset > stream.Length)
            return Option.Create(xe);
        
        var position = stream.Position;
        var absoluteOffset = instance.PayloadOffset + stringOffset;
        stream.Seek(absoluteOffset, SeekOrigin.Begin);
        
        var value = stream.ReadStringZ();
        xe.Add(value.RemoveAll(XDocumentLibrary.InvalidXmlCharacters));
        
        stream.Seek(position, SeekOrigin.Begin);

        return Option.Create(xe);
    }
    
    public static Option<XElement> BitfieldToXElement(this AdfV04Instance instance, Stream stream, AdfV04Type adfType, string name)
    {
        var value = stream.ReadBit();

        var oxe = XElementBuilder.Create(AdfV04MemberLibrary.XName)
            .WithAttribute("name", name)
            .WithAttribute("type", adfType.SafeName)
            .WithAttribute("typeHash", adfType.TypeHash.ToString())
            .WithAttribute("offset", $"{stream.Position:X08}")
            .WithContent($"{value:X01}")
            .BuildOption();

        return oxe;
    }
    
    public static Option<XElement> EnumToXElement(this AdfV04Instance instance, Stream stream, AdfV04Type adfType, string name)
    {
        var value = stream.Read<uint>();

        var oxe = XElementBuilder.Create(AdfV04MemberLibrary.XName)
            .WithAttribute("name", name)
            .WithAttribute("type", adfType.SafeName)
            .WithAttribute("typeHash", adfType.TypeHash.ToString())
            .WithAttribute("offset", $"{stream.Position:X08}")
            .WithContent(value.ToString())
            .BuildOption();

        return oxe;
    }
    
    public static Option<XElement> StringHashToXElement(this AdfV04Instance instance, Stream stream, AdfV04Type adfType, string name, Dictionary<uint, string> stringHashes)
    {
        var offset = stream.Position;
        var stringHash = stream.Read<uint>();
        var optionValue = stringHashes.GetValueOrNone(stringHash);

        if (!optionValue.IsSome(out var value))
            value = $"{stringHash:X08}";

        var oxe = XElementBuilder.Create(AdfV04MemberLibrary.XName)
            .WithAttribute("name", name)
            .WithAttribute("type", adfType.SafeName)
            .WithAttribute("typeHash", adfType.TypeHash.ToString())
            .WithAttribute("hash", optionValue.MapOr(v => Option<string>.None, Option.Some("1")))
            .WithAttribute("offset", $"{offset:X08}")
            .WithContent(value)
            .BuildOption();

        return oxe;
    }

    public static Option<XElement> PointerToXElement(this AdfV04Instance instance, Stream stream, AdfV04Type adfType, string name, AdfV04Type[] types, Dictionary<uint, string> stringHashes)
    {
        var xe = new XElement(adfType.Type.ToXName());
        xe.SetAttributeValue("name", name);
        xe.SetAttributeValue("type", adfType.SafeName);
        xe.SetAttributeValue("typeHash", adfType.TypeHash.ToString());
        xe.SetAttributeValue("offset", $"{stream.Position:X08}");
        
        var dataOffset = stream.Read<uint>();
        
        var position = stream.Position;
        var absoluteOffset = instance.PayloadOffset + dataOffset;
        stream.Seek(absoluteOffset, SeekOrigin.Begin);
        
        var typeHash = adfType.Type == EAdfV04Type.Pointer ? adfType.ScalarTypeHash : stream.Read<uint>();
        
        var optionSubType = types.FirstOrNone(t => t.TypeHash == typeHash);
        if (!optionSubType.IsSome(out var subtype))
            return Option.Create(xe);
        
        var optionXMember = instance.DataToXElement(stream, subtype, subtype.SafeName, types, stringHashes);
        if (!optionXMember.IsSome(out var xMember))
            return Option<XElement>.None;
        
        xe.Add(xMember);
        
        stream.Seek(position, SeekOrigin.Begin);

        return Option.Create(xe);
    }
    
    #endregion

    #region From XElement

    public static Option<Exception> FromXElement(XElement xe, Stream stream, Dictionary<uint, string> stringHashes, string[] stringTable, AdfV04Type[] types, ref int contentOffset)
    {
        var xce = xe.Elements();
        foreach (var childElement in xce)
        {
            var optionException = DataFromXElement(childElement, stream, stringHashes, stringTable, types, ref contentOffset);
            if (optionException.IsSome(out _))
            {
                return optionException;
            }
        }

        return Option.None<Exception>();
    }

    public static Option<Exception> DataFromXElement(XElement xe, Stream stream, Dictionary<uint, string> stringHashes, string[] stringTable, AdfV04Type[] types, ref int contentOffset)
    {
        var resultAdfType = xe.GetAdfV04Type(types);
        if (!resultAdfType.IsOk(out var adfType))
        {
            return (new Exception($"{xe.Name.LocalName} type was missing from type list")).AsOption();
        }

        var result = Option.None<Exception>();
        switch (adfType.Type)
        {
            case EAdfV04Type.Scalar:
                result = ScalarFromXElement(xe, stream, types);
                break;
            case EAdfV04Type.Struct:
                result = StructFromXElement(xe, stream, stringHashes, stringTable, types, ref contentOffset);
                break;
            case EAdfV04Type.Pointer:
                break;
            case EAdfV04Type.Array:
                result = ArrayFromXElement(xe, stream, stringHashes, stringTable, types, ref contentOffset);
                break;
            case EAdfV04Type.InlineArray:
                result = InlineArrayFromXElement(xe, stream, types, ref contentOffset);
                break;
            case EAdfV04Type.String:
                result = StringFromXElement(xe, stream, ref contentOffset);
                break;
            case EAdfV04Type.Recursive:
                break;
            case EAdfV04Type.Bitfield:
                result = BitfieldFromXElement(xe, stream, ref contentOffset);
                break;
            case EAdfV04Type.Enum:
                break;
            case EAdfV04Type.StringHash:
                result = StringHashFromXElement(xe, stream);
                break;
            case EAdfV04Type.Deferred:
                break;
            default:
                return (new Exception()).AsOption();
        }

        return result;
    }

    public static Option<Exception> ScalarFromXElement(XElement xe, Stream stream, AdfV04Type[] types)
    {
        var optionXType = xe.GetAttribute("type");
        if (!optionXType.IsSome(out var xType))
        {
            return (new Exception($"{xe.Name.LocalName} type attribute was missing from struct")).AsOption();
        }
        
        var optionAdfType = types.FirstOrNone(t => t.Name.Equals(xType, StringComparison.InvariantCultureIgnoreCase));
        if (!optionAdfType.IsSome(out var adfType))
        {
            return (new Exception($"{xe.Name.LocalName} type was missing from type list")).AsOption();
        }
        
        return ScalarFromContent(xe.Value, stream, adfType);
    }
    
    public static Option<Exception> ScalarFromContent(string content, Stream stream, AdfV04Type adfType)
    {
        stream.AlignWrite(adfType.Alignment, 0x00);

        switch (adfType.ScalarType)
        {
            case EAdfV04ScalarType.Signed:
                switch (adfType.Size)
                {
                    case sizeof(sbyte):
                    {
                        if (!sbyte.TryParse(content, out var scalar))
                        {
                            return (new Exception($"Failed to cast content {content} to sbyte")).AsOption();
                        }
                        stream.Write(scalar);
                        break;
                    }
                    case sizeof(short):
                    {
                        if (!short.TryParse(content, out var scalar))
                        {
                            return (new Exception($"Failed to cast content {content} to short")).AsOption();
                        }
                        stream.Write(scalar);
                        break;
                    }
                    case sizeof(int):
                    {
                        if (!int.TryParse(content, out var scalar))
                        {
                            return (new Exception($"Failed to cast content {content} to int")).AsOption();
                        }
                        stream.Write(scalar);
                        break;
                    }
                    case sizeof(long):
                    {
                        if (!long.TryParse(content, out var scalar))
                        {
                            return (new Exception($"Failed to cast content {content} to long")).AsOption();
                        }
                        stream.Write(scalar);
                        break;
                    }
                }
                break;
            case EAdfV04ScalarType.Unsigned:
                switch (adfType.Size)
                {
                    case sizeof(byte):
                    {
                        if (!byte.TryParse(content, out var scalar))
                        {
                            return (new Exception($"Failed to cast content {content} to byte")).AsOption();
                        }
                        stream.Write(scalar);
                        break;
                    }
                    case sizeof(ushort):
                    {
                        if (!ushort.TryParse(content, out var scalar))
                        {
                            return (new Exception($"Failed to cast content {content} to ushort")).AsOption();
                        }
                        stream.Write(scalar);
                        break;
                    }
                    case sizeof(uint):
                    {
                        if (!uint.TryParse(content, out var scalar))
                        {
                            return (new Exception($"Failed to cast content {content} to uint")).AsOption();
                        }
                        stream.Write(scalar);
                        break;
                    }
                    case sizeof(ulong):
                    {
                        if (!ulong.TryParse(content, out var scalar))
                        {
                            return (new Exception($"Failed to cast content {content} to ulong")).AsOption();
                        }
                        stream.Write(scalar);
                        break;
                    }
                }
                break;
            case EAdfV04ScalarType.Float:
                switch (adfType.Size)
                {
                    case sizeof(float):
                    {
                        if (!float.TryParse(content, out var scalar))
                        {
                            return (new Exception($"Failed to cast content {content} to float")).AsOption();
                        }
                        stream.Write(scalar);
                        break;
                    }
                    case sizeof(double):
                    {
                        if (!double.TryParse(content, out var scalar))
                        {
                            return (new Exception($"Failed to cast content {content} to double")).AsOption();
                        }
                        stream.Write(scalar);
                        break;
                    }
                }
                break;
            default:
                return (new Exception($"Invalid ScalarType")).AsOption();
        }
        
        return Option.None<Exception>();
    }
    
    public static Option<Exception> StructFromXElement(XElement xe, Stream stream, Dictionary<uint, string> stringHashes,
        string[] stringTable, AdfV04Type[] types, ref int contentOffset)
    {
        var resultAdfType = xe.GetAdfV04Type(types);
        if (!resultAdfType.IsOk(out var adfType))
        {
            return (new Exception($"{xe.Name.LocalName} type was missing from type list")).AsOption();
        }
        
        stream.AlignWrite(adfType.Alignment, 0x00);
        
        var optionException = FromXElement(xe, stream, stringHashes, stringTable, types, ref contentOffset);
        if (optionException.IsSome(out _))
        {
            return optionException;
        }
        
        stream.AlignWrite(adfType.Alignment, 0x00);
        
        return Option.None<Exception>();
    }

    public static Option<Exception> ArrayFromXElement(XElement xe, Stream stream, Dictionary<uint, string> stringHashes,
        string[] stringTable, AdfV04Type[] types, ref int contentOffset)
    {
        var xce = xe.Elements().ToArray();
        
        stream.AlignWrite(8, 0x00);

        if (xce.Length == 0)
        {
            stream.Write<uint>(0);
            stream.Write<float>(0);
            stream.Write<uint>(0);
            stream.Write<float>(0);
            
            return Option.None<Exception>();
        }

        stream.Write(contentOffset);
        stream.Write<float>(0);
        stream.Write(xce.Length);
        stream.Write<float>(0);
        
        var originalPosition = stream.Position;
        stream.Seek(contentOffset, SeekOrigin.Begin);
        
        if (xce.Length != 0)
        {
            var resultChildAdfType = xce[0].GetAdfV04Type(types);
            if (!resultChildAdfType.IsOk(out var childAdfType))
            {
                return (new Exception($"Child {xe.Name.LocalName} type was missing from type list")).AsOption();
            }
            
            contentOffset += (int) (childAdfType.Size * xce.Length);
        }
        
        var optionException = FromXElement(xe, stream, stringHashes, stringTable, types, ref contentOffset);
        if (!optionException.IsNone)
        {
            return optionException;
        }
        
        // contentOffset = (int) stream.Position;
        stream.Seek(originalPosition, SeekOrigin.Begin);
        
        return Option.None<Exception>();
    }

    public static Option<Exception> InlineArrayFromXElement(XElement xe, Stream stream, AdfV04Type[] types, ref int contentOffset)
    {
        var resultAdfType = xe.GetAdfV04Type(types);
        if (!resultAdfType.IsOk(out var adfType))
        {
            return (new Exception($"{xe.Name.LocalName} type was missing from type list")).AsOption();
        }
        
        var content = xe.Value;
        var valueArray = content.Split(' ');

        if (valueArray.Length != adfType.BitCountOrArrayLength)
        {
            return (new Exception($"{xe.Name.LocalName} content length != adfType length {adfType.BitCountOrArrayLength}")).AsOption();
        }

        var optionSubType = types.FirstOrNone(t => t.TypeHash == adfType.ScalarTypeHash);
        if (!optionSubType.IsSome(out var subtype))
        {
            return (new Exception($"{xe.Name.LocalName} could not find subtype")).AsOption();
        }
        
        stream.AlignWrite(adfType.Alignment, 0x00);
        
        foreach (var value in valueArray)
        {
            var optionResult = ScalarFromContent(value, stream, subtype);
            if (!optionResult.IsNone)
            {
                return optionResult;
            }
        }
        
        return Option.None<Exception>();
    }
    
    public static Option<Exception> StringFromXElement(XElement xe, Stream stream, ref int contentOffset)
    {
        stream.AlignWrite(8, 0x00);
        stream.Write((uint) contentOffset);
        stream.Write<uint>(0);
        
        var originalPosition = stream.Position;
        stream.Seek(contentOffset, SeekOrigin.Begin);
        
        stream.Write(Encoding.UTF8.GetBytes(xe.Value));
        stream.Write((byte) 0x00);
        stream.AlignWrite(8, 0x00);
        
        contentOffset = (int) stream.Position;
        stream.Seek(originalPosition, SeekOrigin.Begin);
        
        return Option.None<Exception>();
    }

    public static Option<Exception> BitfieldFromXElement(XElement xe, Stream stream, ref int contentOffset)
    {
        // todo: back read last byte, add this bit, write byte
        return Option.None<Exception>();
    }
    
    public static Option<Exception> StringHashFromXElement(XElement xe, Stream stream)
    {
        stream.AlignWrite(4, 0x00);
        
        if (xe.GetAttribute("hash").IsSome(out var isHashValue))
        {
            if (uint.TryParse(isHashValue, out var isHash) && isHash != 0)
            {
                if (uint.TryParse(xe.Value, NumberStyles.HexNumber, null, out var hash))
                {
                    stream.Write(hash);
                    return Option.None<Exception>();
                }
            }
        }
        
        stream.Write(xe.Value.HashJenkins());
        
        return Option.None<Exception>();
    }
    
    #endregion

    public static Result<AdfV04Type, Exception> GetType(this AdfV04Instance instance, AdfV04Type[] types)
    {
        var optionAdfType = types.FirstOrNone(t => t.TypeHash == instance.TypeHash);
        if (!optionAdfType.IsSome(out var adfType))
        {
            return Result.Err<AdfV04Type>(new InvalidOperationException($"{instance.Name} type was missing from type list"));
        }

        return Result.OkExn(adfType);
    }

    public static void TryFindName(this AdfV04Instance instance, string[] stringTable)
    {
        if ((uint) instance.NameIndex >= stringTable.Length)
            return;
        
        instance.Name = stringTable[instance.NameIndex];
    }
}