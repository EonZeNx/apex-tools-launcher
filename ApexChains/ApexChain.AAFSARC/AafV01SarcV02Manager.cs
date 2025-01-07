using ApexFormat.AAF.V01;
using ApexFormat.SARC.V02;
using ApexFormat.SARC.V02.Class;
using ApexToolsLauncher.Core.Class;
using RustyOptions;

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
        using var inStream = new FileStream(inFilePath, FileMode.Open);
        using var sarcStream = new MemoryStream();

        var aafV01File = new AafV01File();
        var aafResult = aafV01File.ExtractStreamToStream(inStream, sarcStream);
        if (aafResult.IsErr(out _))
            return -1;
        
        sarcStream.Seek(0, SeekOrigin.Begin);
        
        var outDirectoryPath = Path.GetDirectoryName(inFilePath);
        if (!string.IsNullOrEmpty(outDirectory) && Directory.Exists(outDirectory))
            outDirectoryPath = outDirectory;
        
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(inFilePath);
        var directoryPath = Path.Join(outDirectoryPath, fileNameWithoutExtension);
            
        if (!Directory.Exists(directoryPath))
            Directory.CreateDirectory(directoryPath);

        var sarcV02File = new SarcV02File();
        var sarcResult = sarcV02File.ExtractStreamToPath(sarcStream, directoryPath);
        if (sarcResult.IsErr(out _))
            return -1;
        
        var tocPath = $"{inFilePath}.toc";
        if (File.Exists(tocPath))
        {
            using var tocStream = new FileStream(tocPath, FileMode.Open);
            var tocResult = tocStream.ReadSarcV02Toc()
                .OkOr<SarcV02Toc, Exception>(new InvalidOperationException())
                .Map(_ => 0);
            if (tocResult.IsErr(out _))
                return -1;
        }
        
        return 0;
    }

    public string GetProcessorName()
    {
        return $"AAF v{AafV01FileLibrary.Version:D2} - SARC v{SarcV02FileLibrary.Version:D2} chain";
    }
}