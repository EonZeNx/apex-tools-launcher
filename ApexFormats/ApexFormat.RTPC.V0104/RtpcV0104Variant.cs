using System.Xml.Linq;
using ATL.Core.Extensions;
using CommunityToolkit.HighPerformance;

namespace ApexFormat.RTPC.V0104;

public class RtpcV0104Variant
{
    public uint NameHash = 0;
    public ERtpcV0104VariantType VariantType = ERtpcV0104VariantType.Unassigned;
    public object? Data = null;
}

public static class RtpcV0104VariantExtensions
{
    public static RtpcV0104Variant ReadRtpcV0104Variant(this Stream stream)
    {
        var result = new RtpcV0104Variant
        {
            NameHash = stream.Read<uint>(),
            VariantType = stream.Read<ERtpcV0104VariantType>(),
        };

        switch (result.VariantType)
        {
            case ERtpcV0104VariantType.Integer32:
                result.Data = stream.Read<int>();
                break;
            case ERtpcV0104VariantType.Float32:
                result.Data = stream.Read<float>();
                break;
            case ERtpcV0104VariantType.String:
                var stringLength = stream.Read<ushort>();
                result.Data = stream.ReadStringOfLength(stringLength);
                break;
            case ERtpcV0104VariantType.Vector2:
                result.Data = stream.ReadArray<float>(2);
                break;
            case ERtpcV0104VariantType.Vector3:
                result.Data = stream.ReadArray<float>(3);
                break;
            case ERtpcV0104VariantType.Vector4:
                result.Data = stream.ReadArray<float>(4);
                break;
            case ERtpcV0104VariantType.DoNotUse01:
                break;
            case ERtpcV0104VariantType.Matrix3X4:
                result.Data = stream.ReadArray<float>(12);
                break;
            case ERtpcV0104VariantType.Events:
                var count = stream.Read<uint>();
                var values = new (uint, uint)[count];
    
                for (var i = 0; i < count; i++)
                {
                    values[i] = (stream.Read<uint>(), stream.Read<uint>());
                }

                result.Data = values;
                break;
            case ERtpcV0104VariantType.Unassigned:
            default:
                throw new ArgumentOutOfRangeException();
        }

        return result;
    }

    public static XElement WriteXElement(this RtpcV0104Variant variant)
    {
        var xe = new XElement("value");
        xe.SetAttributeValue("id", $"{variant.NameHash:X8}");
        xe.SetAttributeValue("type", variant.VariantType.XmlString());

        if (variant.Data is null) return xe;

        switch (variant.VariantType)
        {
            case ERtpcV0104VariantType.Integer32:
            case ERtpcV0104VariantType.Float32:
            case ERtpcV0104VariantType.String:
                xe.SetValue(variant.Data);
                break;
            case ERtpcV0104VariantType.Vector2:
            case ERtpcV0104VariantType.Vector3:
            case ERtpcV0104VariantType.Vector4:
                var vec = (float[]) variant.Data;
                xe.SetValue(string.Join(",", vec));
                break;
            case ERtpcV0104VariantType.Matrix3X4:
                var mat = (float[]) variant.Data;
                xe.SetValue(string.Join(",", mat));
                break;
            case ERtpcV0104VariantType.Events:
                var eventPairs = ((uint, uint)[]) variant.Data;
                var events = eventPairs.Select(e => $"{e.Item1:X8}={e.Item2:X8}");
                xe.SetValue(string.Join(", ", events));
                break;
            case ERtpcV0104VariantType.Unassigned:
                break;
            case ERtpcV0104VariantType.DoNotUse01:
            default:
                throw new ArgumentOutOfRangeException();
        }

        return xe;
    }
}