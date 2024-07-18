using CommunityToolkit.HighPerformance;

namespace ApexFormat.TAB.V02;

public static class TabV02HeaderConstants
{
    public static uint Magic = 0x00424154; // "TAB"
    public static ushort MajorVersion = 2;
    public static ushort MinorVersion = 1;
    public static int Alignment = 0x1000;
}

/// <summary>
/// Structure:
/// <br/>Magic - <see cref="uint"/>
/// <br/>MajorVersion - <see cref="ushort"/>
/// <br/>MinorVersion - <see cref="ushort"/>
/// <br/>Alignment - <see cref="int"/>
/// </summary>
public class TabV02Header
{
    public uint Magic = TabV02HeaderConstants.Magic;
    public ushort MajorVersion = TabV02HeaderConstants.MajorVersion;
    public ushort MinorVersion = TabV02HeaderConstants.MinorVersion;
    public int Alignment = TabV02HeaderConstants.Alignment;
}

public static class TabV02HeaderExtensions
{
    public static TabV02Header ReadTabV02Header(this Stream stream)
    {
        var result = new TabV02Header
        {
            Magic = stream.Read<uint>(),
            MajorVersion = stream.Read<ushort>(),
            MinorVersion = stream.Read<ushort>(),
            Alignment = stream.Read<int>(),
        };

        if (result.Magic != TabV02HeaderConstants.Magic)
        {
            throw new FileLoadException($"{nameof(result.Magic)} is {result.Magic}, expected {TabV02HeaderConstants.Magic}");
        }

        if (result.MajorVersion != TabV02HeaderConstants.MajorVersion)
        {
            throw new FileLoadException($"{nameof(result.MajorVersion)} is {result.MajorVersion}, expected {TabV02HeaderConstants.MajorVersion}");
        }

        return result;
    }
}