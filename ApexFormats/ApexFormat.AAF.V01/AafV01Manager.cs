using Ionic.Zlib;

namespace ApexFormat.AAF.V01;

public static class AafV01Manager
{
    public static int Decompress(Stream inBuffer, Stream outBuffer)
    {
        if (inBuffer.Length == 0)
        {
            return -1;
        }

        var header = inBuffer.ReadAafV01Header();
        if (header.Magic != AafV01HeaderConstants.Magic)
        {
            return -2;
        }
        
        outBuffer.SetLength(header.TotalUnpackedSize);
        for (var i = 0; i < header.NumChunks; i++)
        {
            var startPosition = inBuffer.Position;
            var chunk = inBuffer.ReadAafV01Chunk();
            if (chunk.Magic != AafV01ChunkConstants.Magic)
            {
                return -3;
            }

            var chunkData = new byte[chunk.CompressedSize];
            inBuffer.ReadExactly(chunkData, 0, (int) chunk.CompressedSize);

            byte[] decompressedData;
            using (var ms = new MemoryStream())
            {
                // Write valid header for ZLib
                ms.WriteByte(0x78);
                // Write compression level
                ms.WriteByte(0x01);
            
                ms.Write(chunkData);
            
                decompressedData = ZlibStream.UncompressBuffer(ms.ToArray());
            }

            if (decompressedData.Length != chunk.DecompressedSize)
            {
                return -4;
            }
            
            outBuffer.Write(decompressedData);
            inBuffer.Seek(startPosition + chunk.ChunkSize, SeekOrigin.Begin);
        }

        return 0;
    }

    public static int Compress(Stream inBuffer, Stream outBuffer)
    {
        return 0;
    }
}
