using ApexToolsLauncher.Core.Extensions;
using CommunityToolkit.HighPerformance;
using RustyOptions;

namespace ApexFormat.SARC.V03.Class;

/// <summary>
/// Structure:
/// <br/>PathOffset - <see cref="uint"/>
/// <br/>DataOffset - <see cref="uint"/>
/// <br/>Size - <see cref="uint"/>
/// <br/>Unk0 - <see cref="uint"/>
/// <br/>Unk1 - <see cref="uint"/>
/// </summary>
public class SarcV03Entry
{
    public uint PathOffset = 0;
    public uint DataOffset = 0;
    public uint Size = 0;
    public uint Unk0 = 0;
    public uint Unk1 = 0;
    
    public string Path = string.Empty;
    
    public bool LocalData => DataOffset != 0 && Size != 0;
}

public static class SarcV03EntryLibrary
{
    public const int SizeOf = sizeof(uint) // PathOffset
                              + sizeof(uint) // DataOffset
                              + sizeof(uint) // Size
                              + sizeof(uint) // Unk0
                              + sizeof(uint); // Unk1
    
    public static Option<SarcV03Entry> ReadSarcV03Entry(this Stream stream)
    {
        if (stream.Length - stream.Position < SizeOf)
        {
            return Option<SarcV03Entry>.None;
        }
        
        var result = new SarcV03Entry
        {
            PathOffset = stream.Read<uint>(),
            DataOffset = stream.Read<uint>(),
            Size = stream.Read<uint>(),
            Unk0 = stream.Read<uint>(),
            Unk1 = stream.Read<uint>(),
        };

        if (result.PathOffset < stream.Length)
        {
            var position = stream.Position;
            stream.Position = SarcV03HeaderLibrary.SizeOf + result.PathOffset;
            result.Path = stream.ReadStringZ();
            stream.Position = position;
        }

        return Option.Some(result);
    }
}
