using ApexFormat.AAF.V01.Class;
using ApexToolsLauncher.Core.Class;
using Ionic.Zlib;
using RustyOptions;

namespace ApexFormat.AAF.V01;

public class AafV01File : ICanExtractPath, IExtractPathToPath, IExtractStreamToStream, ICanRepackPath, IRepackPathToPath, IRepackStreamToStream
{
    protected string ExtractExtension { get; set; } = "ee";

    public bool CanExtractPath(string path)
    {
        if (!File.Exists(path))
            return false;
        
        using var stream = new FileStream(path, FileMode.Open);

        var result = false;
        try
        {
            result = !stream.ReadAafV01Header().IsNone;
        }
        catch (Exception)
        {
        }

        return result;
    }

    public Result<int, Exception> ExtractPathToPath(string inPath, string outPath)
    {
        using var inStream = new FileStream(inPath, FileMode.Open);
        
        ExtractExtension = Path.GetExtension(inPath).Trim('.');
        
        var outDirectoryPath = Path.GetDirectoryName(inPath);
        if (!string.IsNullOrEmpty(outPath) && Directory.Exists(outPath))
            outDirectoryPath = outPath;
        
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(inPath);
        var sarcFilePath = Path.Join(outDirectoryPath, $"{fileNameWithoutExtension}.sarc");
        using var outStream = new FileStream(outPath, FileMode.Open);
        
        var result = ExtractStreamToStream(inStream, outStream);
        
        return result;
    }

    public Result<int, Exception> ExtractStreamToStream(Stream inStream, Stream outStream)
    {
        if (inStream.Length == 0) 
            return Result.Err<int>(new InvalidOperationException($"{nameof(inStream)} is empty"));

        var optionHeader = inStream.ReadAafV01Header();
        if (!optionHeader.IsSome(out var header))
            return Result.Err<int>(new InvalidOperationException($"Failed to read file header"));
        
        outStream.SetLength(header.TotalUnpackedSize);
        for (var i = 0; i < header.ChunkCount; i++)
        {
            var startPosition = inStream.Position;
            var optionChunk = inStream.ReadAafV01Chunk();
            if (!optionChunk.IsSome(out var chunk))
                continue;

            var chunkData = new byte[chunk.CompressedSize];
            inStream.ReadExactly(chunkData, 0, (int) chunk.CompressedSize);

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
                return Result.Err<int>(new InvalidOperationException($"DecompressedSize does not match ({chunk.DecompressedSize} vs actual {decompressedData.Length})"));
            
            outStream.Write(decompressedData);
            inStream.Seek(startPosition + chunk.ChunkSize, SeekOrigin.Begin);
        }

        return Result.OkExn(0);
    }

    public bool CanRepackPath(string path)
    {
        return false;
    }

    public Result<int, Exception> RepackPathToPath(string inPath, string outPath)
    {
        return Result.Err<int>(new NotImplementedException());
    }

    public Result<int, Exception> RepackStreamToStream(Stream inStream, Stream outStream)
    {
        return Result.Err<int>(new NotImplementedException());
    }
}

public static class AafV01FileLibrary
{
    public const int Version = 1;
}
