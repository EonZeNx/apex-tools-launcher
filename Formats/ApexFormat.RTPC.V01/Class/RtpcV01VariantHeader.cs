using ApexFormat.RTPC.V01.Enum;
using ApexToolsLauncher.Core.Class;
using ApexToolsLauncher.Core.Extensions;
using CommunityToolkit.HighPerformance;
using RustyOptions;

namespace ApexFormat.RTPC.V01.Class;

/// <summary>
/// Structure:
/// <br/>NameHash - <see cref="uint"/>
/// <br/>Data - <see cref="byte"/>[4]
/// <br/>VariantType - <see cref="ERtpcV01VariantType"/>
/// </summary>
public class RtpcV01VariantHeader : ISizeOf
{
    public uint NameHash = 0;
    public byte[] Data = [4];
    public ERtpcV01VariantType VariantType = ERtpcV01VariantType.Unassigned;
    
    public static uint SizeOf()
    {
        return sizeof(uint) + // NameHash
               4 + // Data
               sizeof(ERtpcV01VariantType);  // VariantType
    }

    public override string ToString()
    {
        return $"{NameHash:X8} ({VariantType})";
    }
}

public static class RtpcV01VariantHeaderExtensions
{
    public static Option<RtpcV01VariantHeader> ReadRtpcV01VariantHeader(this Stream stream)
    {
        if (stream.Length - stream.Position < RtpcV01VariantHeader.SizeOf())
        {
            return Option<RtpcV01VariantHeader>.None;
        }
        
        var result = new RtpcV01VariantHeader
        {
            NameHash = stream.Read<uint>(),
            Data = stream.ReadBytes(4),
            VariantType = stream.Read<ERtpcV01VariantType>(),
        };

        return Option.Some(result);
    }
}