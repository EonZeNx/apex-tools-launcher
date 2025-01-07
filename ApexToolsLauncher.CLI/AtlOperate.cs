using System;
using System.IO;
using ApexChain.AAFSARC;
using ApexFormat.AAF.V01;
using ApexFormat.ADF.V04;
using ApexFormat.AVTX.V01;
using ApexFormat.IC.V01;
using ApexFormat.RTPC.V01;
using ApexFormat.RTPC.V03;
using ApexFormat.SARC.V02;
using ApexFormat.TAB.V02;
using ApexToolsLauncher.Core.Class;
using RustyOptions;

namespace ApexToolsLauncher.CLI;

public static class AtlOperate
{
    public static Option<IProcessBasic> GetOperator(string path)
    {
        if (!Path.Exists(path))
            return Option<IProcessBasic>.None;

        IProcessBasic? manager = null;
        if (TabV02Manager.CanProcess(path))
        {
            manager = new TabV02Manager();
        }
        if (AafV01SarcV02Manager.CanProcess(path))
        {
            manager = new AafV01SarcV02Manager();
        }
        if (SarcV02Manager.CanProcess(path))
        {
            manager = new SarcV02Manager();
        }
        if (AafV01Manager.CanProcess(path))
        {
            manager = new AafV01Manager();
        }
        if (AdfV04Manager.CanProcess(path))
        {
            manager = new AdfV04Manager();
        }
        if (AvtxV01Manager.CanProcess(path))
        {
            manager = new AvtxV01Manager();
        }
        if (RtpcV01Manager.CanProcess(path))
        {
            manager = new RtpcV01Manager();
        }
        if (RtpcV03Manager.CanProcess(path))
        {
            manager = new RtpcV03Manager();
        }
        if (IcV01Manager.CanProcess(path))
        {
            manager = new IcV01Manager();
        }
        // if (Path.GetExtension(path) == ".xml")

        return Option.Create(manager);
    }
    
    public static void RunOperator(string path, IProcessBasic manager, string outDirectory)
    {
        var absoluteOutDirectory = GetAbsoluteDirectory(path, outDirectory);
        manager.ProcessBasic(path, absoluteOutDirectory);
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