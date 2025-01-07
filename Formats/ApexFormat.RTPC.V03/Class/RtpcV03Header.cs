using ApexToolsLauncher.Core.Extensions;
using CommunityToolkit.HighPerformance;
using RustyOptions;

namespace ApexFormat.RTPC.V03.Class;

/// <summary>
/// Structure:
/// <br/>Magic - <see cref="uint"/>
/// <br/>MajorVersion - <see cref="ushort"/>
/// <br/>MinorVersion - <see cref="ushort"/>
/// </summary>
public class RtpcV03Header
{
    public uint Magic = RtpcV03HeaderLibrary.Magic;
    public uint Version = RtpcV03HeaderLibrary.Version;
}

public static class RtpcV03HeaderLibrary
{
    public const int SizeOf = sizeof(uint) // Magic
                              + sizeof(uint); // Version
    
    public const uint Magic = 0x43505452; // RTPC
    public const ushort Version = 0x03;
    
    public static Option<RtpcV03Header> ReadRtpcV03Header(this Stream stream)
    {
        if (!stream.CouldRead(SizeOf))
        {
            return Option<RtpcV03Header>.None;
        }
        
        var result = new RtpcV03Header
        {
            Magic = stream.Read<uint>(),
            Version = stream.Read<uint>(),
        };

        if (result.Magic != Magic)
        {
            return Option<RtpcV03Header>.None;
        }

        if (result.Version != Version)
        {
            return Option<RtpcV03Header>.None;
        }

        return Option.Some(result);
    }
}