using System.Text;

namespace ApexFormat.AAF.V01;

public static class AafV01ChunkConstants
{
    public const uint Magic = 0x4D415745; // "EWAM"
    public const uint Size = 4 + 4 + 4 + 4;
}

public class AafV01Chunk
{
    public uint CompressedSize = 0;
    public uint DecompressedSize = 0;
    public uint ChunkSize = 0;
    public uint Magic = AafV01ChunkConstants.Magic;
}

public static class AafV01ChunkExtensions
{
    public static AafV01Chunk ReadAafV01Chunk(this Stream stream)
    {
        using var br = new BinaryReader(stream, Encoding.UTF8, true);

        var chunk = new AafV01Chunk
        {
            CompressedSize = br.ReadUInt32(),
            DecompressedSize = br.ReadUInt32(),
            ChunkSize = br.ReadUInt32(),
            Magic = br.ReadUInt32(),
        };

        return chunk;
    }
}