using ApexFormat.AAF.V01.Class;
using ApexToolsLauncher.Core.Class;

namespace ApexFormat.AAF.V01;

public class AafV01Manager : ICanProcessStream, ICanProcessPath, IProcessBasic
{
    public static bool CanProcess(Stream stream)
    {
        return !stream.ReadAafV01Header().IsNone;
    }
    
    public static bool CanProcess(string path)
    {
        if (!File.Exists(path))
            return false;
        
        var file = new AafV01File();
        return file.CanExtractPath(path) || file.CanRepackPath(path);
    }
    
    public int ProcessBasic(string inFilePath, string outDirectory)
    {
        var file = new AafV01File();

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
        return "AAF v01";
    }
}
