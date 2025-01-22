using ApexFormat.SARC.V03.Class;
using ApexToolsLauncher.Core.Class;

namespace ApexFormat.SARC.V03;

public class SarcV03Manager : ICanProcessStream, ICanProcessPath, IProcessBasic
{
    public static bool CanProcess(Stream stream)
    {
        return !stream.ReadSarcV03Header().IsNone;
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

    public int ProcessBasic(string inFilePath, string outDirectory)
    {
        var file = new SarcV03File();

        var result = -1;
        if (file.CanExtractPath(inFilePath))
        {
            var extractResult = file.ExtractPathToPath(inFilePath, outDirectory);
            extractResult.IsOk(out result);
        }
        else if (file.CanRepackPath(inFilePath))
        {
            var repackResult = file.RepackPathToPath(inFilePath, outDirectory);
            repackResult.IsOk(out result);
        }
        
        return result;
    }

    public string GetProcessorName()
    {
        return SarcV03FileLibrary.VersionName;
    }
}
