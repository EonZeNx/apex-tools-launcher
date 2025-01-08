using System.Xml;
using System.Xml.Linq;
using ApexFormat.IC.V01.Class;
using ApexToolsLauncher.Core.Class;
using ApexToolsLauncher.Core.Libraries;

namespace ApexFormat.IC.V01;

public class IcV01File : ICanExtractPath, IExtractPathToPath, IExtractStreamToStream, ICanRepackPath, IRepackPathToPath
{
    protected string ExtractExtension { get; set; } = "bin";
    protected string RepackExtension { get; set; } = "xml";
    
    public bool CanExtractPath(string path)
    {
        if (!File.Exists(path))
            return false;
        
        using var fileStream = new FileStream(path, FileMode.Open);
        return !fileStream.ReadIcV01Instance().IsNone;
    }

    public int ExtractStreamToStream(Stream inStream, Stream outStream)
    {
        List<IcV01Instance> instances = [];
        while (inStream.Position < inStream.Length)
        {
            var optionInstance = inStream.ReadIcV01Instance();
            if (!optionInstance.IsSome(out var instance))
            {
                break;
            }
            
            instances.Add(instance);
        }
        
        var outer = new XElement("instances");
        outer.SetAttributeValue("extension", ExtractExtension);

        foreach (var instance in instances)
        {
            outer.Add(instance.ToXElement());
        }
        
        var xd = new XDocument(XDocumentLibrary.ProjectComment(), outer);
        using var xw = XmlWriter.Create(inStream, new XmlWriterSettings
        {
            Indent = true,
            IndentChars = "  "
        });
        xd.Save(xw);

        return 0;
    }

    public int ExtractPathToPath(string inPath, string outPath)
    {
        using var inStream = new FileStream(inPath, FileMode.Open);
        
        ExtractExtension = Path.GetExtension(inPath).Trim('.');
        var fileName = Path.GetFileNameWithoutExtension(inPath);
        var xmlFilePath = Path.Join(outPath, $"{fileName}.xml");
        
        using var outStream = new FileStream(xmlFilePath, FileMode.Create);
        var result = ExtractStreamToStream(inStream, outStream);
        
        return result;
    }

    public bool CanRepackPath(string path)
    {
        throw new NotImplementedException();
    }

    public int RepackPathToPath(string inPath, string outPath)
    {
        throw new NotImplementedException();
    }
}