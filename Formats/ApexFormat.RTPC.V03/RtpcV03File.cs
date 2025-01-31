using System.Xml;
using ApexFormat.RTPC.V03.Class;
using ApexToolsLauncher.Core.Class;
using ApexToolsLauncher.Core.Libraries;
using ApexToolsLauncher.Core.Libraries.XBuilder;
using RustyOptions;

namespace ApexFormat.RTPC.V03;

public class RtpcV03File : ICanExtractPath, IExtractPathToPath, IExtractStreamToStream, ICanRepackPath, IRepackPathToPath, IRepackStreamToStream
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
            result = !fileStream.ReadRtpcV03Header().IsNone;
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
        var optionHeader = inStream.ReadRtpcV03Header();
        if (!optionHeader.IsSome(out var header))
            return Result.Err<int>(new InvalidOperationException($"Failed to extract {nameof(RtpcV03Header)}"));
        
        var optionContainer = inStream.ReadRtpcV03Container();
        if (!optionContainer.IsSome(out var container))
            return Result.Err<int>(new InvalidOperationException($"Failed to extract {nameof(RtpcV03Container)}"));

        var xd = XProjectBuilder.Create()
            .WithType(RtpcV03FileLibrary.XName)
            .WithVersion(RtpcV03FileLibrary.Version.ToString())
            .WithExtension(ExtractExtension)
            .WithChild(container.ToXElement())
            .Build();

        using var xw = XmlWriter.Create(outStream, XDocumentLibrary.XmlWriterSettings);
        xd.Save(xw);

        return Result.OkExn(0);
    }

    public bool CanRepackPath(string path)
    {
        return false;
    }

    public Result<int, Exception> RepackPathToPath(string inPath, string outPath)
    {
        return Result.Err<int>(new NotImplementedException());
    }

    public Result<int, Exception> RepackStreamToStream(Stream inStream, Stream outStream)
    {
        return Result.Err<int>(new NotImplementedException());
    }
}

public static class RtpcV03FileLibrary
{
    public const string XName = "rtpc";
    public const int Version = 3;

    public static string VersionName = $"{XName.ToUpper()} v{Version:D2}";
}
