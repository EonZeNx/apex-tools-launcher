using ApexToolsLauncher.Core.Extensions;
using CommunityToolkit.HighPerformance;
using RustyOptions;

namespace ApexFormat.AAF.V01.Class;

/// <summary>
/// Structure:
/// <br/>CompressedSize - <see cref="uint"/>
/// <br/>DecompressedSize - <see cref="uint"/>
/// <br/>ChunkSize - <see cref="uint"/>
/// <br/>Magic - <see cref="uint"/>
/// </summary>
public class AafV01Chunk
{
    public uint CompressedSize = 0;
    public uint DecompressedSize = 0;
    public uint ChunkSize = 0;
    public uint Magic = AafV01ChunkLibrary.Magic;
}

public static class AafV01ChunkLibrary
{
    public const int SizeOf = sizeof(uint) // CompressedSize
                              + sizeof(uint) // DecompressedSize
                              + sizeof(uint) // ChunkSize
                              + sizeof(uint); // Magic
    
    public const uint Magic = 0x4D415745; // "EWAM"
    
    public static Option<AafV01Chunk> ReadAafV01Chunk(this Stream stream)
    {
        if (stream.CouldRead(SizeOf))
        {
            return Option<AafV01Chunk>.None;
        }
        
        var result = new AafV01Chunk
        {
            CompressedSize = stream.Read<uint>(),
            DecompressedSize = stream.Read<uint>(),
            ChunkSize = stream.Read<uint>(),
            Magic = stream.Read<uint>(),
        };
        
        if (result.Magic != Magic)
        {
            return Option<AafV01Chunk>.None;
        }

        return Option.Some(result);
    }
}