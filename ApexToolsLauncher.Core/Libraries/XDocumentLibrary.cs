using System.Xml;
using System.Xml.Linq;
using RustyOptions;

namespace ApexToolsLauncher.Core.Libraries;

public static class XDocumentLibrary
{
    public static readonly string[] InvalidXmlCharacters = ["<", ">", "&", "\'", "\"", "#", "\0"];

    public static readonly XmlWriterSettings XmlWriterSettings = new()
    {
        Indent = true,
        IndentChars = "\t"
    };

    public static XComment ProjectXComment() => new($" Generated by {ConstantsLibrary.AppTitle} {ConstantsLibrary.AppVersion} ");

    public static Option<string> GetAttributeOrNone(this XElement xe, string attributeName)
    {
        var attribute = xe.Attribute(attributeName);
        return Option.Create(attribute?.Value);
    }
    
    public static int SortNameThenId(XElement x, XElement y)
    {
        var a1 = (string?) x.Attribute("name");
        var a2 = (string?) y.Attribute("name");
        
        if (!string.IsNullOrEmpty(a1) && !string.IsNullOrEmpty(a2))
        { // a1 name, a2 name
            return string.CompareOrdinal(a1, a2);
        }
        
        if (!string.IsNullOrEmpty(a1) && string.IsNullOrEmpty(a2))
        { // a1 hash?, a2 name
            return -1;
        }
        
        if (string.IsNullOrEmpty(a1) && !string.IsNullOrEmpty(a2))
        { // a1 name, a2 hash?
            return 1;
        }
        
        a1 = (string?) x.Attribute("id");
        a2 = (string?) y.Attribute("id");
        
        if (!string.IsNullOrEmpty(a1) && !string.IsNullOrEmpty(a2))
        { // a1 hash, a2 hash
            return string.CompareOrdinal(a1, a2);
        }
        
        if (!string.IsNullOrEmpty(a1) && string.IsNullOrEmpty(a2))
        { // a1 hash, a2 null
            return -1;
        }
        
        if (string.IsNullOrEmpty(a1) && !string.IsNullOrEmpty(a2))
        { // a1 null, a2 hash
            return 1;
        }

        return 0;
    }
}