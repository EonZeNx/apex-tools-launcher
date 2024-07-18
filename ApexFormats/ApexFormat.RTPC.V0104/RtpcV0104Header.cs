using CommunityToolkit.HighPerformance;

namespace ApexFormat.RTPC.V0104;

public static class RtpcV0104HeaderConstants
{
    public const byte MajorVersion = 0x01;
    public const ushort MinorVersion = 0x04;
}

/// <summary>
/// Structure:
/// <br/>Version01 - <see cref="byte"/>
/// <br/>Version02 - <see cref="ushort"/>
/// <br/>ContainerCount - <see cref="ushort"/>
/// </summary>
public class RtpcV0104Header
{
    public byte MajorVersion = RtpcV0104HeaderConstants.MajorVersion;
    public ushort MinorVersion = RtpcV0104HeaderConstants.MinorVersion;
    public ushort ContainerCount = 0;
}

public static class RtpcV0104HeaderExtensions
{
    public static RtpcV0104Header ReadRtpcV0104Header(this Stream stream)
    {
        var result = new RtpcV0104Header
        {
            MajorVersion = (byte) stream.ReadByte(),
            MinorVersion = stream.Read<ushort>(),
            ContainerCount = stream.Read<ushort>(),
        };

        if (result.MajorVersion != RtpcV0104HeaderConstants.MajorVersion)
        {
            throw new FileLoadException($"{nameof(result.MajorVersion)} is {result.MajorVersion}, expected {RtpcV0104HeaderConstants.MajorVersion}");
        }

        if (result.MinorVersion != RtpcV0104HeaderConstants.MinorVersion)
        {
            throw new FileLoadException($"{nameof(result.MinorVersion)} is {result.MinorVersion}, expected {RtpcV0104HeaderConstants.MinorVersion}");
        }

        return result;
    }
}