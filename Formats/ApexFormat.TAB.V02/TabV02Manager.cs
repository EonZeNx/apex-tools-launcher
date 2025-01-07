using ApexFormat.TAB.V02.Class;
using ApexToolsLauncher.Core.Class;

namespace ApexFormat.TAB.V02;

public class TabV02Manager : ICanProcessStream, ICanProcessPath, IProcessBasic
{
    public static bool CanProcess(Stream stream)
    {
        return !stream.ReadTabV02Header().IsNone;
    }
    
    public static bool CanProcess(string path)
    {
        var file = new TabV02File();
        return file.CanExtractPath(path) || file.CanRepackPath(path);
    }
    
    public int ProcessBasic(string inFilePath, string outDirectory)
    {
        var file = new TabV02File();

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
        return $"TAB v{TabV02FileLibrary.Version:D2}";
    }
}
