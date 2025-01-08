using System.Xml.Linq;
using ApexFormat.RTPC.V01.Enum;
using ApexToolsLauncher.Core.Extensions;
using ApexToolsLauncher.Core.Hash;
using CommunityToolkit.HighPerformance;
using RustyOptions;

namespace ApexFormat.RTPC.V01.Class;

/// <summary>
/// Structure:
/// <br/>NameHash - <see cref="uint"/>
/// <br/>Data - <see cref="byte"/>[4]
/// <br/>VariantType - <see cref="ERtpcV01Variant"/>
/// </summary>
public class RtpcV01Property
{
    public uint NameHash = 0;
    public byte[] Data = [4];
    public ERtpcV01Variant Variant = ERtpcV01Variant.Unassigned;
    public object? DeferredData = null;

    public override string ToString()
    {
        var valueStr = $"{NameHash:X08} ({Variant})";
        if (DeferredData is null)
        {
            return valueStr;
        }
        
        switch (Variant)
        {
            case ERtpcV01Variant.UInteger32:
                valueStr = BitConverter.ToUInt32(Data).ToString();
                break;
            case ERtpcV01Variant.Float32:
                valueStr = BitConverter.ToSingle(Data).ToString();
                break;
            case ERtpcV01Variant.String:
                valueStr = (string) DeferredData;
                break;
            case ERtpcV01Variant.Vector2:
            case ERtpcV01Variant.Vector3:
            case ERtpcV01Variant.Vector4:
            case ERtpcV01Variant.Matrix3X3:
            case ERtpcV01Variant.Matrix4X4:
            case ERtpcV01Variant.Float32Array:
                var floats = (float[]) DeferredData;
                valueStr = string.Join(",", floats);
                break;
            case ERtpcV01Variant.UInteger32Array:
                var uints = (uint[]) DeferredData;
                valueStr = string.Join(",", uints);
                break;
            case ERtpcV01Variant.ByteArray:
                var bytes = (byte[]) DeferredData;
                valueStr = string.Join(",", bytes.Select(b => $"{b:X2}"));
                break;
            case ERtpcV01Variant.ObjectId:
                var oid = (RtpcV01ObjectId) DeferredData;
                valueStr = oid.ToString();
                break;
            case ERtpcV01Variant.Events:
                var eventPairs = ((uint, uint)[]) DeferredData;
                var events = eventPairs.Select(e => $"{e.Item1:X8}={e.Item2:X8}");
                valueStr = string.Join(", ", events);
                break;
            case ERtpcV01Variant.Unassigned:
            case ERtpcV01Variant.Deprecated:
            case ERtpcV01Variant.Total:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        
        return valueStr + $" \"{valueStr}\"";
    }
}

public static class RtpcV01PropertyLibrary
{
    public const int SizeOf = sizeof(uint) // NameHash
                              + 4 // Data
                              + sizeof(ERtpcV01Variant);  // VariantType
    
    public static Option<RtpcV01Property> ReadRtpcV01Property(this Stream stream)
    {
        if (!stream.CouldRead(SizeOf))
        {
            return Option<RtpcV01Property>.None;
        }
        
        var result = new RtpcV01Property
        {
            NameHash = stream.Read<uint>(),
            Data = stream.ReadBytes(4),
            Variant = stream.Read<ERtpcV01Variant>(),
        };
        
        if (result.Variant.IsPrimitive())
            return Option.Some(result);

        var originalPosition = stream.Position;
        var offset = BitConverter.ToUInt32(result.Data);
        stream.Seek(offset, SeekOrigin.Begin);
        
        switch (result.Variant)
        {
            case ERtpcV01Variant.String:
                result.DeferredData = stream.ReadStringZ();
                break;
            case ERtpcV01Variant.Vector2:
                result.DeferredData = stream.ReadArray<float>(2);
                break;
            case ERtpcV01Variant.Vector3:
                result.DeferredData = stream.ReadArray<float>(3);
                break;
            case ERtpcV01Variant.Vector4:
                result.DeferredData = stream.ReadArray<float>(4);
                break;
            case ERtpcV01Variant.Matrix3X3:
                result.DeferredData = stream.ReadArray<float>(9);
                break;
            case ERtpcV01Variant.Matrix4X4:
                result.DeferredData = stream.ReadArray<float>(16);
                break;
            case ERtpcV01Variant.UInteger32Array:
                result.DeferredData = stream.ReadArrayLengthPrefix<uint>();
                break;
            case ERtpcV01Variant.Float32Array:
                result.DeferredData = stream.ReadArrayLengthPrefix<float>();
                break;
            case ERtpcV01Variant.ByteArray:
                result.DeferredData = stream.ReadArrayLengthPrefix<byte>();
                break;
            case ERtpcV01Variant.ObjectId:
                result.DeferredData = stream.ReadRtpcV01ObjectId();
                break;
            case ERtpcV01Variant.Events:
                var count = stream.Read<uint>();
                var values = new (uint, uint)[count];
            
                for (var i = 0; i < count; i++)
                {
                    values[i] = (stream.Read<uint>(), stream.Read<uint>());
                }

                result.DeferredData = values;
                break;
            case ERtpcV01Variant.Unassigned:
            case ERtpcV01Variant.Total:
            default:
                throw new ArgumentOutOfRangeException();
        }

        stream.Seek(originalPosition, SeekOrigin.Begin);
        return Option.Some(result);
    }

    public static XElement WriteXElement(this RtpcV01Property property)
    {
        var xe = new XElement("value");
        
        var optionHashResult = HashDatabases.Lookup(property.NameHash, EHashType.FilePath);
        if (optionHashResult.IsSome(out var hashResult))
        {
            xe.SetAttributeValue("name", hashResult.Value);
        }
        else
        {
            xe.SetAttributeValue("id", $"{property.NameHash:X8}");
        }
        
        xe.SetAttributeValue("type", property.Variant.XmlString());

        if (property.Variant.IsPrimitive())
        {
            switch (property.Variant)
            {
            case ERtpcV01Variant.Unassigned:
                xe.SetValue(BitConverter.ToUInt32(property.Data));
                break;
            case ERtpcV01Variant.UInteger32:
                xe.SetValue(BitConverter.ToUInt32(property.Data));
                break;
            case ERtpcV01Variant.Float32:
                xe.SetValue(BitConverter.ToSingle(property.Data));
                break;
            case ERtpcV01Variant.Total:
                xe.SetValue(BitConverter.ToSingle(property.Data));
                break;
            }

            return xe;
        }

        if (property.DeferredData is null) return xe;

        switch (property.Variant)
        {
        case ERtpcV01Variant.String:
            xe.SetValue(property.DeferredData);
            break;
        case ERtpcV01Variant.Vector2:
        case ERtpcV01Variant.Vector3:
        case ERtpcV01Variant.Vector4:
        case ERtpcV01Variant.Float32Array:
            var vec = (float[]) property.DeferredData;
            xe.SetValue(string.Join(",", vec));
            break;
        case ERtpcV01Variant.Matrix3X3:
        case ERtpcV01Variant.Matrix4X4:
            var mat = (float[]) property.DeferredData;
            xe.SetValue(string.Join(",", mat));
            break;
        case ERtpcV01Variant.UInteger32Array:
            var ints = (uint[]) property.DeferredData;
            xe.SetValue(string.Join(",", ints));
            break;
        case ERtpcV01Variant.ByteArray:
            var bytes = (byte[]) property.DeferredData;
            xe.SetValue(string.Join(",", bytes.Select(b => $"{b:X2}")));
            break;
        case ERtpcV01Variant.ObjectId:
            var oid = (RtpcV01ObjectId) property.DeferredData;
            xe.SetValue(oid.ToString());
            break;
        case ERtpcV01Variant.Events:
            var eventPairs = ((uint, uint)[]) property.DeferredData;
            var events = eventPairs.Select(e => $"{e.Item1:X8}={e.Item2:X8}");
            xe.SetValue(string.Join(", ", events));
            break;
        case ERtpcV01Variant.Deprecated:
        default:
            throw new ArgumentOutOfRangeException();
        }

        return xe;
    }

    public static uint AsUInt(this RtpcV01Property header)
    {
        if (header.Variant != ERtpcV01Variant.UInteger32)
        {
            return 0;
        }

        return BitConverter.ToUInt32(header.Data);
    }

    public static int AsInt(this RtpcV01Property header)
    {
        if (header.Variant != ERtpcV01Variant.UInteger32)
        {
            return 0;
        }

        return BitConverter.ToInt32(header.Data);
    }

    public static float AsFloat(this RtpcV01Property header)
    {
        if (header.Variant != ERtpcV01Variant.Float32)
        {
            return 0;
        }

        return BitConverter.ToSingle(header.Data);
    }
    
    public static string AsString(this RtpcV01Property header)
    {
        if (header.DeferredData is null) return string.Empty;
        if (header.Variant != ERtpcV01Variant.String)
        {
            return string.Empty;
        }

        return (string) header.DeferredData;
    }

    public static float[] AsFloatArray(this RtpcV01Property header)
    {
        if (header.DeferredData is null) return [];

        return header.Variant switch
        {
            ERtpcV01Variant.Vector2 or
                ERtpcV01Variant.Vector3 or
                ERtpcV01Variant.Vector4 or
                ERtpcV01Variant.Matrix3X3 or
                ERtpcV01Variant.Matrix4X4 or
                ERtpcV01Variant.Float32Array
                => (float[])header.DeferredData,
            _ => []
        };
    }

    public static uint[] AsUIntArray(this RtpcV01Property header)
    {
        if (header.DeferredData is null) return [];
        if (header.Variant != ERtpcV01Variant.UInteger32Array)
        {
            return [];
        }

        return (uint[]) header.DeferredData;
    }

    public static int[] AsIntArray(this RtpcV01Property header)
    {
        if (header.DeferredData is null) return [];
        if (header.Variant != ERtpcV01Variant.UInteger32Array)
        {
            return [];
        }

        return (int[]) header.DeferredData;
    }
}