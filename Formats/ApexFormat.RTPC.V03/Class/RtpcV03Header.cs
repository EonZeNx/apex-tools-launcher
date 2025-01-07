using ApexToolsLauncher.Core.Class;
using CommunityToolkit.HighPerformance;
using RustyOptions;

namespace ApexFormat.RTPC.V03.Class;

public static class RtpcV03HeaderConstants
{
    public const uint Magic = 0x43505452; // RTPC
    public const ushort Version = 0x03;
}

/// <summary>
/// Structure:
/// <br/>Magic - <see cref="uint"/>
/// <br/>MajorVersion - <see cref="ushort"/>
/// <br/>MinorVersion - <see cref="ushort"/>
/// </summary>
public class RtpcV03Header : ISizeOf
{
    public uint Magic = RtpcV03HeaderConstants.Magic;
    public uint Version = RtpcV03HeaderConstants.Version;

    public static uint SizeOf()
    {
        return sizeof(uint) + // Magic
               sizeof(uint); // Version
    }
}

public static class RtpcV03HeaderLibrary
{
    public static Option<RtpcV03Header> ReadRtpcV03Header(this Stream stream)
    {
        if (stream.Length - stream.Position < RtpcV03Header.SizeOf())
        {
            return Option<RtpcV03Header>.None;
        }
        
        var result = new RtpcV03Header
        {
            Magic = stream.Read<uint>(),
            Version = stream.Read<uint>(),
        };

        if (result.Magic != RtpcV03HeaderConstants.Magic)
        {
            return Option<RtpcV03Header>.None;
        }

        if (result.Version != RtpcV03HeaderConstants.Version)
        {
            return Option<RtpcV03Header>.None;
        }

        return Option.Some(result);
    }
}