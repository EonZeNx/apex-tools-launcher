using System.Xml;
using ApexFormat.ADF.V04.Class;
using ApexToolsLauncher.Core.Class;
using ApexToolsLauncher.Core.Libraries;
using ApexToolsLauncher.Core.Libraries.XBuilder;

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

    public static int Decompress(Stream inBuffer, Stream outBuffer)
    {
        if (inBuffer.Length == 0) 
            return -1;

        var optionHeader = inBuffer.ReadAdfV04Header();
        if (!optionHeader.IsSome(out var header))
            return -2;

        var file = new AdfV04File
        {
            Header = header
        };
        file.AddBuiltInTypes();
        
        file.ReadStringHashes(inBuffer);
        var localStringTable = file.ReadStringTable(inBuffer);
        file.ReadTypes(inBuffer, localStringTable);
        // todo: add types from other places
        
        var root = file.WriteXInstances(inBuffer, localStringTable);
        
        // todo: extract extension
        var xd = XProjectBuilder.CreateXProjectBuilder()
            .WithType(AdfV04FileLibrary.XName)
            .WithVersion(AdfV04FileLibrary.Version.ToString())
            .WithExtension("adf")
            .WithRoot(root)
            .Build();

        using var xw = XmlWriter.Create(outBuffer, XDocumentLibrary.XmlWriterSettings);
        
        Console.WriteLine(xd.ToString());
        xd.Save(xw);

        return 0;
    }
    
    public int ProcessBasic(string inFilePath, string outDirectory)
    {
        using var inBuffer = new FileStream(inFilePath, FileMode.Open);
        
        var outDirectoryPath = Path.GetDirectoryName(inFilePath);
        if (!string.IsNullOrEmpty(outDirectory) && Directory.Exists(outDirectory))
            outDirectoryPath = outDirectory;
        
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(inFilePath);
        var xmlFilePath = Path.Join(outDirectoryPath, $"{fileNameWithoutExtension}.xml");
        
        using var outBuffer = new FileStream(xmlFilePath, FileMode.Create);
        var result = Decompress(inBuffer, outBuffer);
        
        return result;
    }

    public string GetProcessorName()
    {
        return "ADF v04";
    }
}
