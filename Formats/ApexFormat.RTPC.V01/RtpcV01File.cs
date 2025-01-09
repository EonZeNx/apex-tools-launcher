using System.Xml;
using System.Xml.Linq;
using ApexFormat.RTPC.V01.Class;
using ApexToolsLauncher.Core.Class;
using ApexToolsLauncher.Core.Libraries;
using RustyOptions;

namespace ApexFormat.RTPC.V01;

public class RtpcV01File : ICanExtractPath, IExtractPathToPath, IExtractStreamToStream, ICanRepackPath, IRepackPathToPath, IRepackStreamToStream
{
    protected string ExtractExtension { get; set; } = "epe";
    
    public bool CanExtractPath(string path)
    {
        if (!File.Exists(path))
            return false;
        
        using var fileStream = new FileStream(path, FileMode.Open);

        var result = false;
        try
        {
            result = !fileStream.ReadRtpcV01Header().IsNone;
        }
        catch (Exception)
        {
        }

        return result;
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

    public Result<int, Exception> ExtractStreamToStream(Stream inStream, Stream outStream)
    {
        var optionHeader = inStream.ReadRtpcV01Header();
        if (!optionHeader.IsSome(out var header))
            return Result.Err<int>(new InvalidOperationException($"Failed to extract {nameof(RtpcV01Header)}"));
        
        var optionContainer = inStream.ReadRtpcV01Container();
        if (!optionContainer.IsSome(out var container))
            return Result.Err<int>(new InvalidOperationException($"Failed to extract {nameof(RtpcV01Container)}"));

        var outer = new XElement(RtpcV01FileLibrary.XName);
        outer.SetAttributeValue("extension", ExtractExtension);
        outer.SetAttributeValue("version", RtpcV01FileLibrary.Version);

        var rootXElement = container.ToXElement();
        outer.Add(rootXElement);
        
        var xd = new XDocument(XDocumentLibrary.ProjectComment(), outer);
        using var xw = XmlWriter.Create(outStream, new XmlWriterSettings
        {
            Indent = true,
            IndentChars = "\t"
        });
        xd.Save(xw);

        return Result.OkExn(0);
    }

    public bool CanRepackPath(string path)
    {
        return false;
    }

    public Result<int, Exception> RepackPathToPath(string inPath, string outPath)
    {
        var xe = XElement.Load(inPath);
        
        if (!string.Equals(xe.Name.LocalName, RtpcV01FileLibrary.XName))
        {
            return Result.Err<int>(new InvalidOperationException($"Element name is {xe.Name.LocalName} not {RtpcV01FileLibrary.XName}"));
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

    public Result<int, Exception> RepackStreamToStream(Stream inStream, Stream outStream)
    {
        var xe = XElement.Load(inStream);
        
        if (!string.Equals(xe.Name.LocalName, RtpcV01FileLibrary.XName))
        {
            return Result.Err<int>(new InvalidOperationException($"Element name is {xe.Name.LocalName} not {RtpcV01FileLibrary.XName}"));
        }

        var optionExtension = xe.GetAttributeOrNone("extension");
        if (optionExtension.IsSome(out var extension))
        {
            ExtractExtension = extension;
        }

        // var xInstanceArray = xe.Elements(IcV01InstanceLibrary.XName).ToArray();
        // foreach (var xi in xInstanceArray)
        // {
        //     var instanceResult = xi.Read<IcV01Instance>();
        //     if (instanceResult.IsErr(out _))
        //     {
        //         return instanceResult.Map(_ => -1);
        //     }
        //
        //     outStream.Write(instanceResult.Unwrap());
        // }

        return Result.OkExn(0);
    }
}

public static class RtpcV01FileLibrary
{
    public const string XName = "entity";
    public const int Version = 1;
}
