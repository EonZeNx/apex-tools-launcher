using ATL.Core.Extensions;
using CommunityToolkit.HighPerformance;

namespace ApexFormat.SARC.V02;

public static class SarcV02ArchiveEntryConstants
{
    public const uint FilePathAlignment = 0x04;
}

/// <summary>
/// Structure:
/// <br/>FilePath Length - <see cref="uint"/>
/// <br/>FilePath - <see cref="string"/>
/// <br/>DataOffset - <see cref="uint"/>
/// <br/>Size - <see cref="uint"/>
/// </summary>
public class SarcV02ArchiveEntry
{
    public string FilePath = "";
    public uint DataOffset = 0;
    public uint Size = 0;
}

public static class SarcV02ArchiveEntryExtensions
{
    public static SarcV02ArchiveEntry ReadSarcV02ArchiveEntry(this Stream stream)
    {
        var result = new SarcV02ArchiveEntry
        {
            FilePath = stream.ReadStringLengthPrefix().Replace("\0", ""),
            DataOffset = stream.Read<uint>(),
            Size = stream.Read<uint>()
        };

        return result;
    }
}