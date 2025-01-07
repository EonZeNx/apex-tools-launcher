﻿using ApexFormat.RTPC.V01.Class;
using ApexToolsLauncher.Core.Class;

namespace ApexFormat.RTPC.V01;

public class RtpcV01Manager : ICanProcessStream, ICanProcessPath, IProcessBasic
{
    public static bool CanProcess(Stream stream)
    {
        return !stream.ReadRtpcV01Header().IsNone;
    }
    
    public static bool CanProcess(string path)
    {
        var file = new RtpcV01File();
        return file.CanExtractPath(path) || file.CanRepackPath(path);
    }
    
    public int ProcessBasic(string inFilePath, string outDirectory)
    {
        var file = new RtpcV01File();

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
        return $"RTPC v{RtpcV01FileLibrary.Version:D2}";
    }
}