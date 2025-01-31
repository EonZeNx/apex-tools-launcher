using ApexToolsLauncher.Core.Extensions;
using CommunityToolkit.HighPerformance;
using RustyOptions;

namespace ApexFormat.SARC.V02.Class;

/// <summary>
/// Structure:
/// <br/>FilePath Length - <see cref="uint"/>
/// <br/>FilePath - <see cref="string"/>
/// <br/>DataOffset - <see cref="uint"/>
/// <br/>Size - <see cref="uint"/>
/// </summary>
public class SarcV02Entry
{
    public string FilePath = string.Empty; // length prefix, min size = uint
    public uint DataOffset = 0;
    public uint Size = 0;
    
    public bool LocalData => DataOffset != 0 && Size != 0;
}

public static class SarcV02ArchiveEntryExtensions
{
    public const int SizeOf = sizeof(uint) // FilePath length
                              + sizeof(uint) // DataOffset
                              + sizeof(uint); // Size
    
    public const uint FilePathAlignment = 0x04;
    
    public static Option<SarcV02Entry> ReadSarcV02Entry(this Stream stream)
    {
        if (!stream.CouldRead(SizeOf))
        {
            return Option<SarcV02Entry>.None;
        }
        
        var result = new SarcV02Entry
        {
            FilePath = stream.ReadStringLengthPrefix().Replace("\0", ""),
            DataOffset = stream.Read<uint>(),
            Size = stream.Read<uint>()
        };

        return Option.Some(result);
    }
}