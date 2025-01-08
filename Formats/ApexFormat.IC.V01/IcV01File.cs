using System.Xml;
using System.Xml.Linq;
using ApexFormat.IC.V01.Class;
using ApexToolsLauncher.Core.Class;
using ApexToolsLauncher.Core.Libraries;
using RustyOptions;

namespace ApexFormat.IC.V01;

public class IcV01File : ICanExtractPath, IExtractPathToPath, IExtractStreamToStream, ICanRepackPath, IRepackPathToPath, IRepackStreamToStream
{
    protected string ExtractExtension { get; set; } = "bin";
    
    public bool CanExtractPath(string path)
    {
        if (!File.Exists(path))
            return false;
        
        using var fileStream = new FileStream(path, FileMode.Open);
        return !fileStream.Read<IcV01Instance>().IsNone;
    }

    public Result<int, Exception> ExtractStreamToStream(Stream inStream, Stream outStream)
    {
        List<IcV01Instance> instances = [];
        while (inStream.Position < inStream.Length)
        {
            var optionInstance = inStream.Read<IcV01Instance>();
            if (!optionInstance.IsSome(out var instance))
            {
                break;
            }
            
            instances.Add(instance);
        }
        
        var outer = new XElement(IcV01FileLibrary.XName);
        outer.SetAttributeValue("extension", ExtractExtension);

        foreach (var instance in instances)
        {
            outer.Add(instance.ToXElement());
        }
        
        var xd = new XDocument(XDocumentLibrary.ProjectComment(), outer);
        using var xw = XmlWriter.Create(outStream, new XmlWriterSettings
        {
            Indent = true,
            IndentChars = "  "
        });
        xd.Save(xw);

        return Result.OkExn(0);
    }

    public Result<int, Exception> ExtractPathToPath(string inPath, string outPath)
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
        if (!File.Exists(path))
            return false;
        
        var xe = XElement.Load(path);
        var xInstances = xe.Descendants(IcV01InstanceLibrary.XName).ToArray();
        
        if (xInstances.Length == 0) return false;

        var instance = new IcV01Instance();
        return xInstances.All(xi => instance.CanRepack(xi).IsOk(out _));
    }

    public int RepackStreamToStream(Stream inStream, Stream outStream)
    {
        throw new NotImplementedException();
    }

    public int RepackPathToPath(string inPath, string outPath)
    {
        throw new NotImplementedException();
    }
}

public static class IcV01FileLibrary
{
    public const string XName = "instances";
}
