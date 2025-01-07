using System.Xml;
using System.Xml.Linq;
using ApexFormat.RTPC.V03.Class;
using ApexToolsLauncher.Core.Class;
using ApexToolsLauncher.Core.Libraries;

namespace ApexFormat.RTPC.V03;

public class RtpcV03Manager : ICanProcessStream, ICanProcessPath, IProcessBasic
{
    public static bool CanProcess(Stream stream)
    {
        return !stream.ReadRtpcV03Header().IsNone;
    }
    
    public static bool CanProcess(string path)
    {
        var file = new RtpcV03File();
        return file.CanExtractPath(path) || file.CanRepackPath(path);
    }
    
    public int ProcessBasic(string inFilePath, string outDirectory)
    {
        var file = new RtpcV03File();

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
        return $"RTPC v{RtpcV03FileLibrary.Version:D2}";
    }
}