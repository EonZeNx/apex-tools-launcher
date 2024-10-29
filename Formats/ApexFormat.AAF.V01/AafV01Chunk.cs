using ATL.Core.Class;
using CommunityToolkit.HighPerformance;
using RustyOptions;

namespace ApexFormat.AAF.V01;

public static class AafV01ChunkConstants
{
    public const uint Magic = 0x4D415745; // "EWAM"
    public const uint Size = 4 + 4 + 4 + 4;
}

/// <summary>
/// Structure:
/// <br/>CompressedSize - <see cref="uint"/>
/// <br/>DecompressedSize - <see cref="uint"/>
/// <br/>ChunkSize - <see cref="uint"/>
/// <br/>Magic - <see cref="uint"/>
/// </summary>
public class AafV01Chunk : ISizeOf
{
    public uint CompressedSize = 0;
    public uint DecompressedSize = 0;
    public uint ChunkSize = 0;
    public uint Magic = AafV01ChunkConstants.Magic;

    public static uint SizeOf() => sizeof(uint) + sizeof(uint) + sizeof(uint) + sizeof(uint);
}

public static class AafV01ChunkExtensions
{
    public static Option<AafV01Chunk> ReadAafV01Chunk(this Stream stream)
    {
        if (stream.Length - stream.Position < AafV01Chunk.SizeOf())
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

        return Option.Some(result);
    }
}