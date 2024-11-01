using System.Xml.Linq;
using ApexFormat.IRTPC.V14.Enum;
using ApexToolsLauncher.Core.Extensions;
using ApexToolsLauncher.Core.Hash;
using CommunityToolkit.HighPerformance;
using RustyOptions;

namespace ApexFormat.IRTPC.V14.Class;

public class IrtpcV14Variant : IrtpcV14VariantHeader
{
    public object? Data = null;
}

public static class IrtpcV14VariantExtensions
{
    public static IrtpcV14Variant HeaderToContainer(this IrtpcV14VariantHeader header)
    {
        var result = new IrtpcV14Variant
        {
            NameHash = header.NameHash,
            VariantType = header.VariantType,
        };

        return result;
    }
    
    public static Option<IrtpcV14Variant> ReadIrtpcV14Variant(this Stream stream)
    {
        var optionContainerHeader = stream.ReadIrtpcV14VariantHeader();
        if (!optionContainerHeader.IsSome(out var containerHeader))
            return Option<IrtpcV14Variant>.None;
        
        var result = containerHeader.HeaderToContainer();
        switch (result.VariantType)
        {
            case EIrtpcV14VariantType.Integer32:
                result.Data = stream.Read<int>();
                break;
            case EIrtpcV14VariantType.Float32:
                result.Data = stream.Read<float>();
                break;
            case EIrtpcV14VariantType.String:
                var stringLength = stream.Read<ushort>();
                result.Data = stream.ReadStringOfLength(stringLength);
                break;
            case EIrtpcV14VariantType.Vector2:
                result.Data = stream.ReadArray<float>(2);
                break;
            case EIrtpcV14VariantType.Vector3:
                result.Data = stream.ReadArray<float>(3);
                break;
            case EIrtpcV14VariantType.Vector4:
                result.Data = stream.ReadArray<float>(4);
                break;
            case EIrtpcV14VariantType.DoNotUse01:
                break;
            case EIrtpcV14VariantType.Matrix3X4:
                result.Data = stream.ReadArray<float>(12);
                break;
            case EIrtpcV14VariantType.Events:
                var count = stream.Read<uint>();
                var values = new (uint, uint)[count];
    
                for (var i = 0; i < count; i++)
                {
                    values[i] = (stream.Read<uint>(), stream.Read<uint>());
                }

                result.Data = values;
                break;
            case EIrtpcV14VariantType.Unassigned:
            default:
                break;
        }

        return Option.Some(result);
    }

    public static XElement WriteXElement(this IrtpcV14Variant variant)
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

        if (variant.Data is null) return xe;

        switch (variant.VariantType)
        {
            case EIrtpcV14VariantType.Integer32:
            case EIrtpcV14VariantType.Float32:
            case EIrtpcV14VariantType.String:
                xe.SetValue(variant.Data);
                break;
            case EIrtpcV14VariantType.Vector2:
            case EIrtpcV14VariantType.Vector3:
            case EIrtpcV14VariantType.Vector4:
                var vec = (float[]) variant.Data;
                xe.SetValue(string.Join(",", vec));
                break;
            case EIrtpcV14VariantType.Matrix3X4:
                var mat = (float[]) variant.Data;
                xe.SetValue(string.Join(",", mat));
                break;
            case EIrtpcV14VariantType.Events:
                var eventPairs = ((uint, uint)[]) variant.Data;
                var events = eventPairs.Select(e => $"{e.Item1:X8}={e.Item2:X8}");
                xe.SetValue(string.Join(", ", events));
                break;
            case EIrtpcV14VariantType.Unassigned:
                break;
            case EIrtpcV14VariantType.DoNotUse01:
            default:
                throw new ArgumentOutOfRangeException();
        }

        return xe;
    }
}