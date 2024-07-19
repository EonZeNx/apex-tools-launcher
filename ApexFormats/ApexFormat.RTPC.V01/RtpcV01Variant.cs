using System.Xml.Linq;
using ATL.Core.Extensions;
using ATL.Core.Hash;
using CommunityToolkit.HighPerformance;

namespace ApexFormat.RTPC.V01;

public class RtpcV01Variant
{
    public uint NameHash = 0;
    public byte[] Data = [4];
    public ERtpcV01VariantType VariantType = ERtpcV01VariantType.Unassigned;
    public object? DeferredData = null;
}

public static class RtpcV01VariantExtensions
{
    public static RtpcV01Variant ReadRtpcV01Variant(this Stream stream)
    {
        var result = new RtpcV01Variant
        {
            NameHash = stream.Read<uint>(),
            Data = stream.ReadBytes(4),
            VariantType = stream.Read<ERtpcV01VariantType>(),
        };

        if (result.VariantType.IsPrimitive()) return result;

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
        return result;
    }

    public static XElement WriteXElement(this RtpcV01Variant variant)
    {
        var xe = new XElement("value");

        var hashResult = LookupHashes.Get(variant.NameHash);
        if (hashResult.Valid())
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
            xe.SetValue(string.Join(",", bytes));
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