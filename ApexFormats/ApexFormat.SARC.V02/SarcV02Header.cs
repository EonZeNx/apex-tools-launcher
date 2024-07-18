using CommunityToolkit.HighPerformance;

namespace ApexFormat.SARC.V02;

public static class SarcV02HeaderConstants
{
    public const uint MagicLength = 0x04;
    public const uint Magic = 0x43524153; // "SARC"
    public const uint Version = 0x02;
}

/// <summary>
/// Structure:
/// <br/>MagicLength - <see cref="uint"/>
/// <br/>Magic - <see cref="uint"/>
/// <br/>Version - <see cref="uint"/>
/// <br/>Size - <see cref="uint"/>
/// </summary>
public class SarcV02Header
{
    public uint MagicLength = SarcV02HeaderConstants.MagicLength;
    public uint Magic = SarcV02HeaderConstants.Magic;
    public uint Version = SarcV02HeaderConstants.Version;
    public uint Size;
}

public static class SarcV02HeaderExtensions
{
    public static SarcV02Header ReadSarcV02Header(this Stream stream)
    {
        var result = new SarcV02Header
        {
            MagicLength = stream.Read<uint>(),
            Magic = stream.Read<uint>(),
            Version = stream.Read<uint>(),
            Size = stream.Read<uint>()
        };

        if (result.MagicLength != SarcV02HeaderConstants.MagicLength)
        {
            throw new FileLoadException($"{nameof(result.MagicLength)} is {result.MagicLength}, expected {SarcV02HeaderConstants.MagicLength}");
        }

        if (result.Magic != SarcV02HeaderConstants.Magic)
        {
            throw new FileLoadException($"{nameof(result.Magic)} is {result.Magic}, expected {SarcV02HeaderConstants.Magic}");
        }

        if (result.Version != SarcV02HeaderConstants.Version)
        {
            throw new FileLoadException($"{nameof(result.Version)} is {result.Version}, expected {SarcV02HeaderConstants.Version}");
        }

        return result;
    }
}