using ApexFormat.IC.V01.Enum;
using ApexToolsLauncher.Core.Class;
using ApexToolsLauncher.Core.Extensions;
using CommunityToolkit.HighPerformance;
using RustyOptions;

namespace ApexFormat.IC.V01.Class;

/// <summary>
/// Structure:
/// <br/>NameHash - <see cref="uint"/>
/// <br/>Variant - <see cref="EIcVariantV01"/>
/// <br/>Data - <see cref="object"/>?
/// </summary>
public class IcV01Property : ISizeOf
{
    public uint NameHash = 0;
    public EIcVariantV01 Variant = EIcVariantV01.Unassigned;
    public object? Data = null;
    
    public static uint SizeOf()
    {
        return sizeof(uint) + // NameHash
               sizeof(EIcVariantV01) + // Variant
               sizeof(uint); // Min data size
    }
}

public static class IcV01PropertyExtensions
{
    public static Option<IcV01Property> ReadIcV01Property(this Stream stream)
    {
        if (stream.Length - stream.Position < IcV01Property.SizeOf())
        {
            return Option<IcV01Property>.None;
        }
        
        var result = new IcV01Property
        {
            NameHash = stream.Read<uint>(),
            Variant = stream.Read<EIcVariantV01>(),
        };
        
        switch (result.Variant)
        {
            case EIcVariantV01.Unassigned:
            case EIcVariantV01.UInteger32:
                result.Data = stream.Read<int>();
                break;
            case EIcVariantV01.Float32:
                result.Data = stream.Read<float>();
                break;
            case EIcVariantV01.String:
                var stringLength = stream.Read<ushort>();
                result.Data = stream.ReadStringOfLength(stringLength);
                break;
            case EIcVariantV01.Vector2:
                result.Data = stream.ReadArray<float>(2);
                break;
            case EIcVariantV01.Vector3:
                result.Data = stream.ReadArray<float>(3);
                break;
            case EIcVariantV01.Vector4:
                result.Data = stream.ReadArray<float>(4);
                break;
            case EIcVariantV01.Matrix3X3:
                result.Data = stream.ReadArray<float>(9);
                break;
            case EIcVariantV01.Matrix3X4:
                result.Data = stream.ReadArray<float>(12);
                break;
            case EIcVariantV01.UInteger32Array:
                result.Data = stream.ReadArrayLengthPrefix<uint>();
                break;
            case EIcVariantV01.Float32Array:
                result.Data = stream.ReadArrayLengthPrefix<float>();
                break;
            case EIcVariantV01.ByteArray:
                result.Data = stream.ReadArrayLengthPrefix<byte>();
                break;
            case EIcVariantV01.ObjectId:
                result.Data = stream.ReadIcV01ObjectId();
                break;
            case EIcVariantV01.Events:
                result.Data = stream.ReadArrayLengthPrefix<(uint, uint)>();
                break;
            default:
                break;
        }

        return Option.Some(result);
    }
}