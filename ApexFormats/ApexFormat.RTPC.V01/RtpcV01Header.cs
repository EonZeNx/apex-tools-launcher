using CommunityToolkit.HighPerformance;

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
/// <br/>Version - <see cref="uint"/>
/// </summary>
public class RtpcV01Header
{
    public uint Magic = RtpcV01HeaderConstants.Magic;
    public ushort MajorVersion = RtpcV01HeaderConstants.MajorVersion;
    public ushort MinorVersion = RtpcV01HeaderConstants.MinorVersion;
}

public static class RtpcV01HeaderExtensions
{
    public static RtpcV01Header ReadRtpcV01Header(this Stream stream)
    {
        var result = new RtpcV01Header
        {
            Magic = stream.Read<uint>(),
            MajorVersion = stream.Read<ushort>(),
            MinorVersion = stream.Read<ushort>(),
        };

        if (result.Magic != RtpcV01HeaderConstants.Magic)
        {
            throw new FileLoadException($"{nameof(result.Magic)} is {result.Magic}, expected {RtpcV01HeaderConstants.Magic}");
        }

        if (result.MajorVersion != RtpcV01HeaderConstants.MajorVersion)
        {
            throw new FileLoadException($"{nameof(result.MajorVersion)} is {result.MajorVersion}, expected {RtpcV01HeaderConstants.MajorVersion}");
        }

        if (result.MinorVersion != RtpcV01HeaderConstants.MinorVersion)
        {
            throw new FileLoadException($"{nameof(result.MinorVersion)} is {result.MinorVersion}, expected {RtpcV01HeaderConstants.MinorVersion}");
        }

        return result;
    }
}