using System.Xml.Linq;
using ApexFormat.RTPC.V03.Enum;
using ApexToolsLauncher.Core.Extensions;
using ApexToolsLauncher.Core.Hash;
using CommunityToolkit.HighPerformance;
using RustyOptions;

namespace ApexFormat.RTPC.V03.Class;

public class RtpcV03Variant : RtpcV03VariantHeader
{
    public object? DeferredData = null;
}

public static class RtpcV03VariantLibrary
{
    public static RtpcV03Variant VariantHeaderToVariant(this RtpcV03VariantHeader header)
    {
        var result = new RtpcV03Variant
        {
            NameHash = header.NameHash,
            Data = header.Data,
            VariantType = header.VariantType,
        };

        return result;
    }
    
    public static Option<RtpcV03Variant> ReadRtpcV03Variant(this Stream stream)
    {
        var optionVariantHeader = stream.ReadRtpcV03VariantHeader();
        if (!optionVariantHeader.IsSome(out var variantHeader))
            return Option<RtpcV03Variant>.None;
        
        var result = variantHeader.VariantHeaderToVariant();
        if (result.VariantType.IsPrimitive())
            return Option.Some(result);

        var originalPosition = stream.Position;
        var offset = BitConverter.ToUInt32(result.Data);
        stream.Seek(offset, SeekOrigin.Begin);
        
        switch (result.VariantType)
        {
        case ERtpcV03VariantType.String:
            result.DeferredData = stream.ReadStringZ();
            break;
        case ERtpcV03VariantType.Vector2:
            result.DeferredData = stream.ReadArray<float>(2);
            break;
        case ERtpcV03VariantType.Vector3:
            result.DeferredData = stream.ReadArray<float>(3);
            break;
        case ERtpcV03VariantType.Vector4:
            result.DeferredData = stream.ReadArray<float>(4);
            break;
        case ERtpcV03VariantType.Matrix3X3:
            result.DeferredData = stream.ReadArray<float>(9);
            break;
        case ERtpcV03VariantType.Matrix4X4:
            result.DeferredData = stream.ReadArray<float>(16);
            break;
        case ERtpcV03VariantType.UInteger32Array:
            result.DeferredData = stream.ReadArrayLengthPrefix<uint>();
            break;
        case ERtpcV03VariantType.Float32Array:
            result.DeferredData = stream.ReadArrayLengthPrefix<float>();
            break;
        case ERtpcV03VariantType.ByteArray:
            result.DeferredData = stream.ReadArrayLengthPrefix<byte>();
            break;
        case ERtpcV03VariantType.ObjectId:
            result.DeferredData = stream.ReadRtpcV01ObjectId();
            break;
        case ERtpcV03VariantType.Events:
            var count = stream.Read<uint>();
            var values = new (uint, uint)[count];
        
            for (var i = 0; i < count; i++)
            {
                values[i] = (stream.Read<uint>(), stream.Read<uint>());
            }

            result.DeferredData = values;
            break;
        case ERtpcV03VariantType.Unassigned:
        case ERtpcV03VariantType.Total:
        default:
            throw new ArgumentOutOfRangeException();
        }

        stream.Seek(originalPosition, SeekOrigin.Begin);
        return Option.Some(result);
    }

    public static XElement WriteXElement(this RtpcV03Variant variant)
    {
        var xe = new XElement("value");
        
        var optionHashResult = HashDatabases.Lookup(variant.NameHash);
        if (optionHashResult.IsSome(out var hashResult))
        {
            xe.SetAttributeValue("name", hashResult.Value);
        }
        else
        {
            xe.SetAttributeValue("id", $"{variant.NameHash:X8}");
        }
        
        xe.SetAttributeValue("type", variant.VariantType.XmlString());

        if (variant.VariantType.IsPrimitive())
        {
            switch (variant.VariantType)
            {
            case ERtpcV03VariantType.Unassigned:
                xe.SetValue(BitConverter.ToUInt32(variant.Data));
                break;
            case ERtpcV03VariantType.UInteger32:
                xe.SetValue(BitConverter.ToUInt32(variant.Data));
                break;
            case ERtpcV03VariantType.Float32:
                xe.SetValue(BitConverter.ToSingle(variant.Data));
                break;
            case ERtpcV03VariantType.Total:
                xe.SetValue(BitConverter.ToSingle(variant.Data));
                break;
            }

            return xe;
        }

        if (variant.DeferredData is null) return xe;

        switch (variant.VariantType)
        {
        case ERtpcV03VariantType.String:
            xe.SetValue(variant.DeferredData);
            break;
        case ERtpcV03VariantType.Vector2:
        case ERtpcV03VariantType.Vector3:
        case ERtpcV03VariantType.Vector4:
        case ERtpcV03VariantType.Float32Array:
            var vec = (float[]) variant.DeferredData;
            xe.SetValue(string.Join(",", vec));
            break;
        case ERtpcV03VariantType.Matrix3X3:
        case ERtpcV03VariantType.Matrix4X4:
            var mat = (float[]) variant.DeferredData;
            xe.SetValue(string.Join(",", mat));
            break;
        case ERtpcV03VariantType.UInteger32Array:
            var ints = (uint[]) variant.DeferredData;
            xe.SetValue(string.Join(",", ints));
            break;
        case ERtpcV03VariantType.ByteArray:
            var bytes = (byte[]) variant.DeferredData;
            xe.SetValue(string.Join(",", bytes.Select(b => $"{b:X2}")));
            break;
        case ERtpcV03VariantType.ObjectId:
            var objectIdValue = (RtpcV03ObjectId) variant.DeferredData;
            var objectId = objectIdValue.String();
            xe.SetValue(objectId);
            break;
        case ERtpcV03VariantType.Events:
            var eventPairs = ((uint, uint)[]) variant.DeferredData;
            var events = eventPairs.Select(e => $"{e.Item1:X8}={e.Item2:X8}");
            xe.SetValue(string.Join(", ", events));
            break;
        case ERtpcV03VariantType.Deprecated:
        default:
            throw new ArgumentOutOfRangeException();
        }

        return xe;
    }
}