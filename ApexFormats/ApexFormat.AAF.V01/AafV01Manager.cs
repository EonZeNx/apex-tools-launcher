using ATL.Core.Class;
using Ionic.Zlib;

namespace ApexFormat.AAF.V01;

public class AafV01Manager : ICanProcessStream, ICanProcessPath, IProcessBasic
{
    public static bool CanProcess(Stream stream)
    {
        return !stream.ReadAafV01Header().IsNone;
    }
    
    public static bool CanProcess(string path)
    {
        if (Directory.Exists(path))
        { // don't support repacking directories just yet
            return false;
        }
        
        if (File.Exists(path))
        {
            using var fileStream = new FileStream(path, FileMode.Open);
            return CanProcess(fileStream);
        }

        return false;
    }
    
    public static int Decompress(Stream inBuffer, Stream outBuffer)
    {
        if (inBuffer.Length == 0) 
            return -1;

        var optionHeader = inBuffer.ReadAafV01Header();
        if (!optionHeader.IsSome(out var header))
            return -2;
        
        outBuffer.SetLength(header.TotalUnpackedSize);
        for (var i = 0; i < header.ChunkCount; i++)
        {
            var startPosition = inBuffer.Position;
            var optionChunk = inBuffer.ReadAafV01Chunk();
            if (!optionChunk.IsSome(out var chunk))
                continue;
            
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

    public int ProcessBasic(string inFilePath, string outDirectory)
    {
        var inBuffer = new FileStream(inFilePath, FileMode.Open);
        
        var outDirectoryPath = Path.GetDirectoryName(inFilePath);
        if (!string.IsNullOrEmpty(outDirectory) && Directory.Exists(outDirectory))
            outDirectoryPath = outDirectory;
        
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(inFilePath);
        var sarcFilePath = Path.Join(outDirectoryPath, $"{fileNameWithoutExtension}.sarc");
        
        var outBuffer = new FileStream(sarcFilePath, FileMode.Create);
        var result = Decompress(inBuffer, outBuffer);
        
        return result;
    }
}
