using ApexFormat.ADF.V04.Class;
using ApexToolsLauncher.Core.Class;

namespace ApexFormat.ADF.V04;

public class AdfV04Manager : ICanProcessStream, ICanProcessPath, IProcessBasic
{
    public static bool CanProcess(Stream stream)
    {
        return !stream.ReadAdfV04Header().IsNone;
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

    public int ProcessBasic(string inFilePath, string outDirectory)
    {
        var file = new AdfV04File();

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
        return AdfV04FileLibrary.VersionName;
    }
}
