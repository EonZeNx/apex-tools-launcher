using ApexFormat.RTPC.V0104.Enum;
using ATL.Core.Class;
using CommunityToolkit.HighPerformance;
using RustyOptions;

namespace ApexFormat.RTPC.V0104.Class;

/// <summary>
/// Structure:
/// <br/>NameHash - <see cref="uint"/>
/// <br/>VariantType - <see cref="ERtpcV0104VariantType"/>
/// </summary>
public class RtpcV0104VariantHeader : ISizeOf
{
    public uint NameHash = 0;
    public ERtpcV0104VariantType VariantType = ERtpcV0104VariantType.Unassigned;

    public static uint SizeOf()
    {
        return sizeof(uint) + // NameHash
               sizeof(ERtpcV0104VariantType); // VariantType
    }
}

public static class RtpcV0104VariantHeaderExtensions
{
    public static Option<RtpcV0104VariantHeader> ReadRtpcV0104VariantHeader(this Stream stream)
    {
        if (stream.Length - stream.Position < RtpcV0104VariantHeader.SizeOf())
        {
            return Option<RtpcV0104VariantHeader>.None;
        }
        
        var result = new RtpcV0104VariantHeader
        {
            NameHash = stream.Read<uint>(),
            VariantType = stream.Read<ERtpcV0104VariantType>(),
        };

        return Option.Some(result);
    }
}