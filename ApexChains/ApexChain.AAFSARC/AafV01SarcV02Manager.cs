using ApexFormat.AAF.V01;
using ApexFormat.SARC.V02;
using ApexToolsLauncher.Core.Class;

namespace ApexChain.AAFSARC;

public class AafV01SarcV02Manager : ICanProcessStream, ICanProcessPath, IProcessBasic
{
    public static bool CanProcess(Stream stream)
    {
        return AafV01Manager.CanProcess(stream);
    }

    public static bool CanProcess(string path)
    {
        return AafV01Manager.CanProcess(path);
    }

    public int ProcessBasic(string inFilePath, string outDirectory)
    {
        using var inBuffer = new FileStream(inFilePath, FileMode.Open);
        using var sarcBuffer = new MemoryStream();

        var result = AafV01Manager.Decompress(inBuffer, sarcBuffer);
        sarcBuffer.Seek(0, SeekOrigin.Begin);
        
        var outDirectoryPath = Path.GetDirectoryName(inFilePath);
        if (!string.IsNullOrEmpty(outDirectory) && Directory.Exists(outDirectory))
            outDirectoryPath = outDirectory;
        
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(inFilePath);
        var directoryPath = Path.Join(outDirectoryPath, fileNameWithoutExtension);
            
        if (!Directory.Exists(directoryPath))
            Directory.CreateDirectory(directoryPath);
        
        SarcV02Manager.Decompress(sarcBuffer, directoryPath);
        
        var tocPath = $"{inFilePath}.toc";
        if (File.Exists(tocPath))
        {
            using var tocBuffer = new FileStream(tocPath, FileMode.Open);
            result = SarcV02Manager.DecompressToc(tocBuffer, directoryPath);
        }
        
        return result;
    }

    public string GetProcessorName()
    {
        return "AAF v01 - SARC v02 chain";
    }
}