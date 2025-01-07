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

        var result = false;
        try
        {
            result = !fileStream.Read<IcV01Instance>().IsNone;
        }
        catch (Exception)
        {
        }

        return result;
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

        XElement xe;
        try
        {
            xe = XElement.Load(path);
        }
        catch (Exception e)
        {
            return false;
        }
        
        var xInstances = xe.Descendants(IcV01InstanceLibrary.XName).ToArray();
        
        if (xInstances.Length == 0) return false;

        var instance = new IcV01Instance();
        return xInstances.All(xi => instance.CanRepack(xi).IsOk(out _));
    }

    public Result<int, Exception> RepackStreamToStream(Stream inStream, Stream outStream)
    {
        var xe = XElement.Load(inStream);
        
        if (!string.Equals(xe.Name.LocalName, IcV01FileLibrary.XName))
        {
            return Result.Err<int>(new InvalidOperationException($"Element nane is {xe.Name.LocalName} not {IcV01FileLibrary.XName}"));
        }

        var optionExtension = xe.GetAttributeOrNone("extension");
        if (optionExtension.IsSome(out var extension))
        {
            ExtractExtension = extension;
        }

        var xInstanceArray = xe.Elements(IcV01InstanceLibrary.XName).ToArray();
        foreach (var xi in xInstanceArray)
        {
            var instanceResult = xi.Read<IcV01Instance>();
            if (instanceResult.IsErr(out _))
            {
                return instanceResult.Map(_ => -1);
            }

            outStream.Write(instanceResult.Unwrap());
        }

        return Result.OkExn(0);
    }

    public Result<int, Exception> RepackPathToPath(string inPath, string outPath)
    {
        var xe = XElement.Load(inPath);
        
        if (!string.Equals(xe.Name.LocalName, IcV01FileLibrary.XName))
        {
            return Result.Err<int>(new InvalidOperationException($"Element nane is {xe.Name.LocalName} not {IcV01FileLibrary.XName}"));
        }

        var optionExtension = xe.GetAttributeOrNone("extension");
        if (optionExtension.IsSome(out var extension))
        {
            ExtractExtension = extension;
        }
        
        var fileName = Path.GetFileNameWithoutExtension(inPath);
        var repackFilePath = Path.Join(outPath, $"{fileName}.{ExtractExtension}");
        
        using var inStream = new FileStream(inPath, FileMode.Open);
        using var outStream = new FileStream(repackFilePath, FileMode.Create);
        
        return RepackStreamToStream(inStream, outStream);
    }
}

public static class IcV01FileLibrary
{
    public const string XName = "instances";
    public const int Version = 1;
}
