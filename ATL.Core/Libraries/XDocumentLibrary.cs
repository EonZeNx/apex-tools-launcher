using System.Xml.Linq;

namespace ATL.Core.Libraries;

public static class XDocumentLibrary
{
    public static XComment AtlGeneratedComment()
    {
        return new XComment($"Generated by {ConstantsLibrary.AppTitle} {ConstantsLibrary.AppVersion}");
    }
}