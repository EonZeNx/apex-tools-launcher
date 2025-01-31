using ApexFormat.IC.V01.Class;
using ApexToolsLauncher.Core.Class;

namespace ApexFormat.IC.V01;

public class IcV01Manager : ICanProcessStream, ICanProcessPath, IProcessBasic
{
    public static bool CanProcess(Stream stream)
    {
        return !stream.Read<IcV01Instance>().IsNone;
    }
    
    public static bool CanProcess(string path)
    {
        var file = new IcV01File();
        return file.CanExtractPath(path) || file.CanRepackPath(path);
    }
    
    public int ProcessBasic(string inFilePath, string outDirectory)
    {
        var file = new IcV01File();

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
        return IcV01FileLibrary.VersionName;
    }
}