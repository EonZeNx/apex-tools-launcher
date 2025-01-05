using System;
using System.Collections.Generic;
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
using ApexToolsLauncher.Core.Libraries;
using HavokFormat.Scene;
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
        if (HkSceneManager.CanProcess(path))
        {
            manager = new HkSceneManager();
        }
        if (IcV01Manager.CanProcess(path))
        {
            manager = new IcV01Manager();
        }
        // if (Path.GetExtension(path) == ".xml")

        return Option.Create(manager);
    }
    
    public static Dictionary<string, Option<IProcessBasic>> GetOperatorForPaths(IEnumerable<string> inPaths)
    {
        var mapped = new Dictionary<string, Option<IProcessBasic>>();

        foreach (var path in inPaths)
        {
            mapped[path] = AtlOperate.GetOperator(path);
        }

        return mapped;
    }

    public static void RunOperator(string path, IProcessBasic manager, string outDirectory)
    {
        var absoluteOutDirectory = GetAbsoluteDirectory(path, outDirectory);
        manager.ProcessBasic(path, absoluteOutDirectory);
    }
    
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
        else if (HkSceneManager.CanProcess(inPath))
        {
            manager = new HkSceneManager();
            message = $"{message} as HkScene";
        }
        else if (IcV01Manager.CanProcess(inPath))
        { // should be last
            manager = new IcV01Manager();
            message = $"{message} as ICv01";
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