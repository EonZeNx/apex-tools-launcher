using System.Xml.Linq;
using ApexFormat.RTPC.V01.Enum;
using ApexToolsLauncher.Core.Extensions;
using ApexToolsLauncher.Core.Hash;
using CommunityToolkit.HighPerformance;
using RustyOptions;

namespace ApexFormat.RTPC.V01.Class;

public class RtpcV01Variant : RtpcV01VariantHeader
{
    public object? DeferredData = null;

    public override string ToString()
    {
        if (DeferredData is null)
            return base.ToString();
        
        var valueStr = "";
        switch (VariantType)
        {
            case ERtpcV01VariantType.UInteger32:
                valueStr = ((uint) DeferredData).ToString();
                break;
            case ERtpcV01VariantType.Float32:
                valueStr = ((float) DeferredData).ToString();
                break;
            case ERtpcV01VariantType.String:
                valueStr = (string) DeferredData;
                break;
            case ERtpcV01VariantType.Vector2:
            case ERtpcV01VariantType.Vector3:
            case ERtpcV01VariantType.Vector4:
            case ERtpcV01VariantType.Matrix3X3:
            case ERtpcV01VariantType.Matrix4X4:
            case ERtpcV01VariantType.Float32Array:
                var floats = (float[]) DeferredData;
                valueStr = string.Join(",", floats);
                break;
            case ERtpcV01VariantType.UInteger32Array:
                var uints = (uint[]) DeferredData;
                valueStr = string.Join(",", uints);
                break;
            case ERtpcV01VariantType.ByteArray:
                var bytes = (byte[]) DeferredData;
                valueStr = string.Join(",", bytes.Select(b => $"{b:X2}"));
                break;
            case ERtpcV01VariantType.ObjectId:
                var objectIdValue = (RtpcV01ObjectId) DeferredData;
                var objectId = objectIdValue.String();
                valueStr = objectId;
                break;
            case ERtpcV01VariantType.Events:
                var eventPairs = ((uint, uint)[]) DeferredData;
                var events = eventPairs.Select(e => $"{e.Item1:X8}={e.Item2:X8}");
                valueStr = string.Join(", ", events);
                break;
            case ERtpcV01VariantType.Unassigned:
            case ERtpcV01VariantType.Deprecated:
            case ERtpcV01VariantType.Total:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        
        return base.ToString() + $"\"{valueStr}\"";
    }
}

public static class RtpcV01VariantExtensions
{
    public static RtpcV01Variant VariantHeaderToVariant(this RtpcV01VariantHeader header)
    {
        var result = new RtpcV01Variant
        {
            NameHash = header.NameHash,
            Data = header.Data,
            VariantType = header.VariantType,
        };

        return result;
    }
    
    public static Option<RtpcV01Variant> ReadRtpcV01Variant(this Stream stream)
    {
        var optionVariantHeader = stream.ReadRtpcV01VariantHeader();
        if (!optionVariantHeader.IsSome(out var variantHeader))
            return Option<RtpcV01Variant>.None;
        
        var result = variantHeader.VariantHeaderToVariant();
        if (result.VariantType.IsPrimitive())
            return Option.Some(result);

        var originalPosition = stream.Position;
        var offset = BitConverter.ToUInt32(result.Data);
        stream.Seek(offset, SeekOrigin.Begin);
        
        switch (result.VariantType)
        {
        case ERtpcV01VariantType.String:
            result.DeferredData = stream.ReadStringZ();
            break;
        case ERtpcV01VariantType.Vector2:
            result.DeferredData = stream.ReadArray<float>(2);
            break;
        case ERtpcV01VariantType.Vector3:
            result.DeferredData = stream.ReadArray<float>(3);
            break;
        case ERtpcV01VariantType.Vector4:
            result.DeferredData = stream.ReadArray<float>(4);
            break;
        case ERtpcV01VariantType.Matrix3X3:
            result.DeferredData = stream.ReadArray<float>(9);
            break;
        case ERtpcV01VariantType.Matrix4X4:
            result.DeferredData = stream.ReadArray<float>(16);
            break;
        case ERtpcV01VariantType.UInteger32Array:
            result.DeferredData = stream.ReadArrayLengthPrefix<uint>();
            break;
        case ERtpcV01VariantType.Float32Array:
            result.DeferredData = stream.ReadArrayLengthPrefix<float>();
            break;
        case ERtpcV01VariantType.ByteArray:
            result.DeferredData = stream.ReadArrayLengthPrefix<byte>();
            break;
        case ERtpcV01VariantType.ObjectId:
            result.DeferredData = stream.ReadRtpcV01ObjectId();
            break;
        case ERtpcV01VariantType.Events:
            var count = stream.Read<uint>();
            var values = new (uint, uint)[count];
        
            for (var i = 0; i < count; i++)
            {
                values[i] = (stream.Read<uint>(), stream.Read<uint>());
            }

            result.DeferredData = values;
            break;
        case ERtpcV01VariantType.Unassigned:
        case ERtpcV01VariantType.Total:
        default:
            throw new ArgumentOutOfRangeException();
        }

        stream.Seek(originalPosition, SeekOrigin.Begin);
        return Option.Some(result);
    }

    public static XElement WriteXElement(this RtpcV01Variant variant)
    {
        var xe = new XElement("value");
        
        var optionHashResult = HashDatabases.Lookup(variant.NameHash, EHashType.FilePath);
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
            case ERtpcV01VariantType.Unassigned:
                xe.SetValue(BitConverter.ToUInt32(variant.Data));
                break;
            case ERtpcV01VariantType.UInteger32:
                xe.SetValue(BitConverter.ToUInt32(variant.Data));
                break;
            case ERtpcV01VariantType.Float32:
                xe.SetValue(BitConverter.ToSingle(variant.Data));
                break;
            case ERtpcV01VariantType.Total:
                xe.SetValue(BitConverter.ToSingle(variant.Data));
                break;
            }

            return xe;
        }

        if (variant.DeferredData is null) return xe;

        switch (variant.VariantType)
        {
        case ERtpcV01VariantType.String:
            xe.SetValue(variant.DeferredData);
            break;
        case ERtpcV01VariantType.Vector2:
        case ERtpcV01VariantType.Vector3:
        case ERtpcV01VariantType.Vector4:
        case ERtpcV01VariantType.Float32Array:
            var vec = (float[]) variant.DeferredData;
            xe.SetValue(string.Join(",", vec));
            break;
        case ERtpcV01VariantType.Matrix3X3:
        case ERtpcV01VariantType.Matrix4X4:
            var mat = (float[]) variant.DeferredData;
            xe.SetValue(string.Join(",", mat));
            break;
        case ERtpcV01VariantType.UInteger32Array:
            var ints = (uint[]) variant.DeferredData;
            xe.SetValue(string.Join(",", ints));
            break;
        case ERtpcV01VariantType.ByteArray:
            var bytes = (byte[]) variant.DeferredData;
            xe.SetValue(string.Join(",", bytes.Select(b => $"{b:X2}")));
            break;
        case ERtpcV01VariantType.ObjectId:
            var objectIdValue = (RtpcV01ObjectId) variant.DeferredData;
            var objectId = objectIdValue.String();
            xe.SetValue(objectId);
            break;
        case ERtpcV01VariantType.Events:
            var eventPairs = ((uint, uint)[]) variant.DeferredData;
            var events = eventPairs.Select(e => $"{e.Item1:X8}={e.Item2:X8}");
            xe.SetValue(string.Join(", ", events));
            break;
        case ERtpcV01VariantType.Deprecated:
        default:
            throw new ArgumentOutOfRangeException();
        }

        return xe;
    }
}