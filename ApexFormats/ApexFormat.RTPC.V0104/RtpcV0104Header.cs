using ATL.Core.Class;
using CommunityToolkit.HighPerformance;
using RustyOptions;

namespace ApexFormat.RTPC.V0104;

public static class RtpcV0104HeaderConstants
{
    public const byte MajorVersion = 0x01;
    public const ushort MinorVersion = 0x04;
}

/// <summary>
/// Structure:
/// <br/>MajorVersion - <see cref="byte"/>
/// <br/>MinorVersion - <see cref="ushort"/>
/// <br/>ContainerCount - <see cref="ushort"/>
/// </summary>
public class RtpcV0104Header : ISizeOf
{
    public byte MajorVersion = RtpcV0104HeaderConstants.MajorVersion;
    public ushort MinorVersion = RtpcV0104HeaderConstants.MinorVersion;
    public ushort ContainerCount = 0;

    public static uint SizeOf()
    {
        return sizeof(byte) + // MajorVersion
               sizeof(ushort) + // MinorVersion
               sizeof(ushort); // ContainerCount
    }
}

public static class RtpcV0104HeaderExtensions
{
    public static Option<RtpcV0104Header> ReadRtpcV0104Header(this Stream stream)
    {
        if (stream.Length - stream.Position < RtpcV0104Header.SizeOf())
        {
            return Option<RtpcV0104Header>.None;
        }

        var result = new RtpcV0104Header
        {
            MajorVersion = (byte) stream.ReadByte(),
            MinorVersion = stream.Read<ushort>(),
            ContainerCount = stream.Read<ushort>(),
        };

        if (result.MajorVersion != RtpcV0104HeaderConstants.MajorVersion)
        {
            return Option<RtpcV0104Header>.None;
        }

        if (result.MinorVersion != RtpcV0104HeaderConstants.MinorVersion)
        {
            return Option<RtpcV0104Header>.None;
        }

        return Option.Some(result);
    }
}