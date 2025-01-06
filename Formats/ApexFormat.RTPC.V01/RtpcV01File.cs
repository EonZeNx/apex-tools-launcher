﻿using System.Xml;
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

        var outer = new XElement("entity");
        outer.SetAttributeValue("extension", ExtractExtension);
        outer.SetAttributeValue("version", "1");

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
        throw new NotImplementedException();
    }

    public Result<int, Exception> RepackPathToPath(string inPath, string outPath)
    {
        throw new NotImplementedException();
    }

    public Result<int, Exception> RepackStreamToStream(Stream inStream, Stream outStream)
    {
        throw new NotImplementedException();
    }
}