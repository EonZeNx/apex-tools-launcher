using System.Xml;
using System.Xml.Linq;
using ApexFormat.ADF.V04.Class;
using ATL.Core.Class;
using ATL.Core.Libraries;

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
        // TODO: Add types from other places
        
        var outer = new XElement("adf");
        outer.SetAttributeValue("extension", "adf");
        outer.SetAttributeValue("version", "4");

        var rootXElement = file.WriteXInstances(inBuffer, localStringTable);
        outer.Add(rootXElement);
        
        var xd = new XDocument(XDocumentLibrary.AtlGeneratedComment(), outer);
        using var xw = XmlWriter.Create(outBuffer, new XmlWriterSettings
        {
            Indent = true,
            IndentChars = "\t"
        });
        
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
}
