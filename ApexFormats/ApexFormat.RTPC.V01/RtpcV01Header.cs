using ATL.Core.Class;
using CommunityToolkit.HighPerformance;
using RustyOptions;

namespace ApexFormat.RTPC.V01;

public static class RtpcV01HeaderConstants
{
    public const uint Magic = 0x43505452; // RTPC
    public const ushort MajorVersion = 0x01;
    public const ushort MinorVersion = 0x00;
}

/// <summary>
/// Structure:
/// <br/>Magic - <see cref="uint"/>
/// <br/>MajorVersion - <see cref="ushort"/>
/// <br/>MinorVersion - <see cref="ushort"/>
/// </summary>
public class RtpcV01Header : ISizeOf
{
    public uint Magic = RtpcV01HeaderConstants.Magic;
    public ushort MajorVersion = RtpcV01HeaderConstants.MajorVersion;
    public ushort MinorVersion = RtpcV01HeaderConstants.MinorVersion;

    public static uint SizeOf()
    {
        return sizeof(uint) + // Magic
               sizeof(ushort) + // MajorVersion
               sizeof(ushort); // MinorVersion
    }
}

public static class RtpcV01HeaderExtensions
{
    public static Option<RtpcV01Header> ReadRtpcV01Header(this Stream stream)
    {
        if (stream.Length - stream.Position < RtpcV01Header.SizeOf())
        {
            return Option<RtpcV01Header>.None;
        }
        
        var result = new RtpcV01Header
        {
            Magic = stream.Read<uint>(),
            MajorVersion = stream.Read<ushort>(),
            MinorVersion = stream.Read<ushort>(),
        };

        if (result.Magic != RtpcV01HeaderConstants.Magic)
        {
            return Option<RtpcV01Header>.None;
        }

        if (result.MajorVersion != RtpcV01HeaderConstants.MajorVersion)
        {
            return Option<RtpcV01Header>.None;
        }

        if (result.MinorVersion != RtpcV01HeaderConstants.MinorVersion)
        {
            return Option<RtpcV01Header>.None;
        }

        return Option.Some(result);
    }
}