using System.Xml;
using System.Xml.Linq;
using ATL.Core.Libraries;

namespace ApexFormat.RTPC.V0104;

public static class RtpcV0104Manager
{
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

        var outer = new XElement("entity");
        outer.SetAttributeValue("extension", "epe");
        outer.SetAttributeValue("format", "RTPC");
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
}