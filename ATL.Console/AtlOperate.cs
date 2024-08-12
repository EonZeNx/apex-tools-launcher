using ApexChain.AAFSARC;
using ApexFormat.AAF.V01;
using ApexFormat.ADF.V04;
using ApexFormat.AVTX.V01;
using ApexFormat.IRTPC.V14;
using ApexFormat.RTPC.V01;
using ApexFormat.RTPC.V03;
using ApexFormat.SARC.V02;
using ApexFormat.TAB.V02;
using ATL.Core.Class;
using ATL.Core.Libraries;

namespace ATL.Console;

public static class AtlOperate
{
    public static void OperateFile(string inPath, string outDirectory)
    {
        var pathName = Path.GetFileName(inPath);
        if (string.IsNullOrEmpty(pathName))
            pathName = Path.GetDirectoryName(inPath);
        
        var message = $"Processing '{pathName}'";

        IProcessBasic manager;
        if (TabV02Manager.CanProcess(inPath))
        {
            manager = new TabV02Manager();
            message = $"{message} as TABv02";
        }
        else if (AafV01SarcV02Manager.CanProcess(inPath))
        {
            manager = new AafV01SarcV02Manager();
            message = $"{message} as AAFv01-SARCv02 chain";
        }
        else if (SarcV02Manager.CanProcess(inPath))
        {
            manager = new SarcV02Manager();
            message = $"{message} as SARCv02";
        }
        else if (AafV01Manager.CanProcess(inPath))
        {
            manager = new AafV01Manager();
            message = $"{message} as AAFv01";
        }
        else if (AdfV04Manager.CanProcess(inPath))
        {
            manager = new AdfV04Manager();
            message = $"{message} as ADFv04";
        }
        else if (AvtxV01Manager.CanProcess(inPath))
        {
            manager = new AvtxV01Manager();
            message = $"{message} as AVTXv01";
        }
        else if (RtpcV01Manager.CanProcess(inPath))
        {
            manager = new RtpcV01Manager();
            message = $"{message} as RTPCv01";
        }
        else if (RtpcV03Manager.CanProcess(inPath))
        {
            manager = new RtpcV03Manager();
            message = $"{message} as RTPCv03";
        }
        else if (IrtpcV14Manager.CanProcess(inPath))
        { // should be last
            manager = new IrtpcV14Manager();
            message = $"{message} as RTPCv0104";
        }
        // Scripts should not be run from here
        // else if (Path.GetExtension(inPath) == ".xml")
        else
        {
            ConsoleLibrary.Log($"File not supported '{pathName}'", LogType.Warning);
            return;
        }
        
        ConsoleLibrary.Log(message, LogType.Info);
        
        var absoluteOutDirectory = GetAbsoluteDirectory(inPath, outDirectory);
        manager.ProcessBasic(inPath, absoluteOutDirectory);
        
        ConsoleLibrary.Log($"Finished '{pathName}'", LogType.Info);
    }
    
    public static string GetAbsoluteDirectory(string inPath, string outDirectory)
    {
        var result = Path.GetDirectoryName(inPath) ?? inPath;
        
        if (!string.IsNullOrEmpty(outDirectory))
        { // outDirectory is valid
            result = Path.IsPathFullyQualified(outDirectory)
                ? outDirectory
                : Path.GetFullPath(outDirectory, AppDomain.CurrentDomain.BaseDirectory);
        }

        return result;
    }
}