using System.Xml;
using System.Xml.Linq;
using ApexFormat.RTPC.V0104.Class;
using ATL.Core.Class;
using ATL.Core.Libraries;

namespace ApexFormat.RTPC.V0104;

public class RtpcV0104Manager : ICanProcessStream, ICanProcessPath, IProcessBasic
{
    public static bool CanProcess(Stream stream)
    {
        return !stream.ReadRtpcV0104Header().IsNone;
    }
    
    public static bool CanProcess(string path)
    {
        if (Directory.Exists(path))
        { // don't support repacking directories just yet
            return false;
        }
        
        if (File.Exists(path))
        {
            using var fileStream = new FileStream(path, FileMode.Open);
            return CanProcess(fileStream);
        }

        return false;
    }
    
    public static int Decompress(Stream inBuffer, Stream outBuffer)
    {
        var optionHeader = inBuffer.ReadRtpcV0104Header();
        if (!optionHeader.IsSome(out var header))
            return -1;

        var containers = new RtpcV0104Container[header.ContainerCount];
        for (var i = 0; i < header.ContainerCount; i++)
        {
            var optionContainer = inBuffer.ReadRtpcV01Container();
            if (optionContainer.IsSome(out var container))
                containers[i] = container;
        }

        var outer = new XElement("inline");
        outer.SetAttributeValue("extension", "bin");
        outer.SetAttributeValue("version", "0104");
    
        var root = new XElement("object");
        root.SetAttributeValue("name", "root");

        foreach (var container in containers)
        {
            var cxe = container.WriteXElement();
            root.Add(cxe);
        }
    
        outer.Add(root);
    
        var xd = new XDocument(XDocumentLibrary.AtlGeneratedComment(), outer);
        using var xw = XmlWriter.Create(outBuffer, new XmlWriterSettings
        {
            Indent = true,
            IndentChars = "\t"
        });
        xd.Save(xw);

        return 0;
    }
    
    public int ProcessBasic(string inFilePath, string outDirectory)
    {
        var inBuffer = new FileStream(inFilePath, FileMode.Open);
        
        var outDirectoryPath = Path.GetDirectoryName(inFilePath);
        if (!string.IsNullOrEmpty(outDirectory) && Directory.Exists(outDirectory))
            outDirectoryPath = outDirectory;
        
        var fileName = Path.GetFileNameWithoutExtension(inFilePath);
        var xmlFilePath = Path.Join(outDirectoryPath, $"{fileName}.xml");
        
        var outBuffer = new FileStream(xmlFilePath, FileMode.Create);
        var result = Decompress(inBuffer, outBuffer);
        
        return result;
    }
}