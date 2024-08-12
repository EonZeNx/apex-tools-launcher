using ApexFormat.AVTX.V01.Class;
using ATL.Core.Class;

namespace ApexFormat.AVTX.V01;

public class AvtxV01Manager : ICanProcessStream, ICanProcessPath, IProcessBasic
{
    public static bool CanProcess(Stream stream)
    {
        return !stream.ReadAvtxV01Header().IsNone;
    }
    
    public static bool CanProcess(string path)
    {
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

        var optionHeader = inBuffer.ReadAvtxV01Header();
        if (!optionHeader.IsSome(out var header))
            return -2;

        var avtxFile = new AvtxV01File
        {
            Header = header
        };
        
        avtxFile.ReadAvtx(inBuffer);

        return -1;
    }
    
    public int ProcessBasic(string inFilePath, string outDirectory)
    {
        using var inBuffer = new FileStream(inFilePath, FileMode.Open);
        
        var outDirectoryPath = Path.GetDirectoryName(inFilePath);
        if (!string.IsNullOrEmpty(outDirectory) && Directory.Exists(outDirectory))
            outDirectoryPath = outDirectory;
        
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(inFilePath);
        var xmlFilePath = Path.Join(outDirectoryPath, $"{fileNameWithoutExtension}.xml");
        
        using var outBuffer = new FileStream(xmlFilePath, FileMode.Create);
        var result = Decompress(inBuffer, outBuffer);
        
        return result;
    }
}
