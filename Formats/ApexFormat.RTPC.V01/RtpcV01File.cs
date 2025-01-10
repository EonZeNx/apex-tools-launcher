using System.Xml;
using System.Xml.Linq;
using ApexToolsLauncher.Core.Class;
using ApexToolsLauncher.Core.Libraries;
using ApexFormat.RTPC.V01.Class;
using ApexToolsLauncher.Core.Extensions;
using RustyOptions;

namespace ApexFormat.RTPC.V01;

public class RtpcV01File : ICanExtractPath, IExtractPathToPath, IExtractStreamToStream, ICanRepackPath, IRepackPathToPath, IRepackStreamToStream
{
    protected string ExtractExtension { get; set; } = "epe";
    
    protected Dictionary<string, uint> StringMap { get; set; } = new();
    
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
        
        var xContainers = xe.Elements(RtpcV01ContainerLibrary.XName).ToArray();
        
        if (xContainers.Length == 0) return false;

        var container = new RtpcV01Container();
        return container.CanRepack(xContainers[0]).IsOk(out _);
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

        RtpcV01HeaderLibrary.Write(outStream);

        var xContainers = xe.Elements(RtpcV01ContainerLibrary.XName).ToArray();
        foreach (var xc in xContainers)
        {
            var containerResult = xc.ToRtpcV01Container();
            if (containerResult.IsErr(out _))
            {
                return containerResult.Map(_ => -1);
            }

            var container = containerResult.Unwrap();
            var originalOffset = outStream.Position;
            
            outStream.Seek(RtpcV01ContainerLibrary.SizeOf, SeekOrigin.Current);
            outStream.WriteData(container, StringMap);
            
            outStream.Seek(originalOffset, SeekOrigin.Begin);
            outStream.Write(container);
        }

        return Result.OkExn(0);
    }
}

public static class RtpcV01FileLibrary
{
    public const string XName = "entity";
    public const int Version = 1;
}
