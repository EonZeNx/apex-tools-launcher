using OodleDotNet;

namespace ApexToolsLauncher.Core.Extensions;

public static class OodleExtensions
{
    public static int Decompress(this Oodle oodle, Stream compressed, int compressedSize, Stream uncompressed, int uncompressedSize)
    {
        var compressedBytes = new byte[compressedSize];
        var compressedReadCount = compressed.Read(compressedBytes, 0, (int) compressedSize);

        if (compressedReadCount < compressedSize)
        {
            return -1;
        }
        
        var uncompressedBytes = new byte[uncompressedSize];
        
        oodle.Decompress(compressedBytes, uncompressedBytes);
        uncompressed.Write(uncompressedBytes);
        
        return 0;
    }

    public static int Decompress(this Oodle oodle, Stream compressed, uint compressedSize, Stream uncompressed,
        uint uncompressedSize)
    {
        return oodle.Decompress(compressed, (int) compressedSize, uncompressed, (int) uncompressedSize);
    }
}