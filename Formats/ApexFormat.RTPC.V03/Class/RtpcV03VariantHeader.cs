using ApexFormat.RTPC.V03.Enum;
using ApexToolsLauncher.Core.Extensions;
using CommunityToolkit.HighPerformance;
using RustyOptions;

namespace ApexFormat.RTPC.V03.Class;

/// <summary>
/// Structure:
/// <br/>NameHash - <see cref="uint"/>
/// <br/>Data - <see cref="byte"/>[4]
/// <br/>VariantType - <see cref="ERtpcV03VariantType"/>
/// </summary>
public class RtpcV03VariantHeader
{
    public uint NameHash = 0;
    public byte[] Data = [4];
    public ERtpcV03VariantType VariantType = ERtpcV03VariantType.Unassigned;
}

public static class RtpcV03VariantHeaderLibrary
{
    public const int SizeOf = sizeof(uint) // NameHash
                              + 4 // Data
                              + sizeof(ERtpcV03VariantType); // VariantType
    
    public static Option<RtpcV03VariantHeader> ReadRtpcV03VariantHeader(this Stream stream)
    {
        if (!stream.CouldRead(SizeOf))
        {
            return Option<RtpcV03VariantHeader>.None;
        }
        
        var result = new RtpcV03VariantHeader
        {
            NameHash = stream.Read<uint>(),
            Data = stream.ReadBytes(4),
            VariantType = stream.Read<ERtpcV03VariantType>(),
        };

        return Option.Some(result);
    }
}