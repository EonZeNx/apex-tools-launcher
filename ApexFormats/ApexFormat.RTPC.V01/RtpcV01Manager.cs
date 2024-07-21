﻿using System.Xml;
using System.Xml.Linq;
using ATL.Core.Class;
using ATL.Core.Libraries;

namespace ApexFormat.RTPC.V01;

public class RtpcV01Manager : ICanProcessStream, ICanProcessPath, IProcessBasic
{
    public static bool CanProcess(Stream stream)
    {
        return !stream.ReadRtpcV01Header().IsNone;
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
        var optionHeader = inBuffer.ReadRtpcV01Header();
        if (!optionHeader.IsSome(out var header))
            return -1;
        
        var optionContainer = inBuffer.ReadRtpcV01Container();
        if (!optionContainer.IsSome(out var container))
            return -2;

        var outer = new XElement("entity");
        outer.SetAttributeValue("extension", "epe");
        outer.SetAttributeValue("format", "RTPC");
        outer.SetAttributeValue("version", "1");

        var rootXElement = container.WriteXElement();
        outer.Add(rootXElement);
        
        var xd = new XDocument(XDocumentLibrary.AtlGeneratedComment(), outer);
        using var xw = XmlWriter.Create(outBuffer, new XmlWriterSettings
        {
            Indent = true,
            IndentChars = "\t"
        });
        xd.Save(xw);

        return 0;
    }
    
    public int ProcessBasic(string inFilePath)
    {
        var inBuffer = new FileStream(inFilePath, FileMode.Open);
        
        var targetFilePath = Path.GetDirectoryName(inFilePath);
        var targetFileName = Path.GetFileNameWithoutExtension(inFilePath);
        var targetXmlFilePath = Path.Join(targetFilePath, $"{targetFileName}.xml");
        var outBuffer = new FileStream(targetXmlFilePath, FileMode.Create);
        
        var result = Decompress(inBuffer, outBuffer);
        return result;
    }
}