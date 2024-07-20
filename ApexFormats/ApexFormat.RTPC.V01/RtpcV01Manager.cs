﻿using System.Xml;
using System.Xml.Linq;
using ATL.Core.Libraries;

namespace ApexFormat.RTPC.V01;

public static class RtpcV01Manager
{
    public static int Decompress(Stream inBuffer, Stream outBuffer)
    {
        var header = inBuffer.ReadRtpcV01Header();
        var rootContainer = inBuffer.ReadRtpcV01Container();

        var outer = new XElement("entity");
        outer.SetAttributeValue("extension", "epe");
        outer.SetAttributeValue("format", "RTPC");
        outer.SetAttributeValue("version", "1");

        var rootXElement = rootContainer.WriteXElement();
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
}