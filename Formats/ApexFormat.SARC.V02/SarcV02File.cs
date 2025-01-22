using System.Xml;
using System.Xml.Linq;
using ApexFormat.SARC.V02.Class;
using ApexToolsLauncher.Core.Class;
using ApexToolsLauncher.Core.Extensions;
using ApexToolsLauncher.Core.Libraries;
using ApexToolsLauncher.Core.Libraries.XBuilder;
using RustyOptions;

namespace ApexFormat.SARC.V02;

public class SarcV02File : ICanExtractPath, IExtractPathToPath, IExtractStreamToPath, ICanRepackPath, IRepackPathToPath, IRepackStreamToStream
{
    protected string ExtractExtension { get; set; } = "ee";

    public bool CanExtractPath(string path)
    {
        if (!File.Exists(path))
            return false;
        
        using var stream = new FileStream(path, FileMode.Open);

        var result = false;
        try
        {
            result = !stream.ReadSarcV02Header().IsNone;
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
        
        var outDirectoryPath = Path.GetDirectoryName(inPath);
        if (!string.IsNullOrEmpty(outPath) && Directory.Exists(outPath))
            outDirectoryPath = outPath;
        
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(inPath);
        var directoryPath = Path.Join(outDirectoryPath, fileNameWithoutExtension);
            
        if (!Directory.Exists(directoryPath))
            Directory.CreateDirectory(directoryPath);

        var result = ExtractStreamToPath(inStream, directoryPath);
        
        var tocPath = $"{inPath}.toc";
        if (File.Exists(tocPath))
        {
            using var tocStream = new FileStream(tocPath, FileMode.Open);
            result = tocStream.ReadSarcV02Toc()
                .OkOr<SarcV02Toc, Exception>(new InvalidOperationException())
                .Map(_ => 0);
        }
        
        return result;
    }
    
    public Result<bool, Exception> WriteEntryFile(string outPath, SarcV02ArchiveEntry[] entries)
    {
        var root = new XElement("files");
        foreach (var archiveEntry in entries)
        {
            var entry = XElementBuilder.Create("file")
                .WithAttribute("size", archiveEntry.Size.ToString())
                .WithAttribute("ref", (archiveEntry.DataOffset == 0).ToString())
                .WithContent(archiveEntry.FilePath)
                .Build();;
            
            root.Add(entry);
        }

        var xmlFilePath = Path.Join(outPath, "@files.xml");
        using var xmlFile = new FileStream(xmlFilePath, FileMode.Create);
        
        var xd = XProjectBuilder.CreateXProjectBuilder()
            .WithType(SarcV02FileLibrary.XName)
            .WithVersion(SarcV02FileLibrary.Version.ToString())
            .WithExtension(ExtractExtension)
            .WithRoot(root)
            .Build();
        
        using var xw = XmlWriter.Create(xmlFile, XDocumentLibrary.XmlWriterSettings);
        xd.Save(xw);

        return Result.OkExn(true);
    }

    public static Option<SarcV02ArchiveEntry[]> ParseFileEntries(Stream stream)
    {
        if (stream.Length == 0)
            return Option<SarcV02ArchiveEntry[]>.None;

        var optionHeader = stream.ReadSarcV02Header();
        if (!optionHeader.IsSome(out var header))
            return Option<SarcV02ArchiveEntry[]>.None;
        
        var archiveEntries = new List<SarcV02ArchiveEntry>();

        var startPosition = stream.Position;
        while (true)
        {
            var optionArchiveEntry = stream.ReadSarcV02ArchiveEntry();
            if (!optionArchiveEntry.IsSome(out var archiveEntry))
                continue;
            
            archiveEntries.Add(archiveEntry);
            if (header.Size - (stream.Position - startPosition) <= 15)
            {
                break;
            }
        }

        return Option.Create(archiveEntries.ToArray());
    }
    
    public static Result<bool, Exception> ReadFileEntry(Stream inBuffer, SarcV02ArchiveEntry archiveEntry, Stream outBuffer)
    {
        if (archiveEntry.DataOffset == 0 || archiveEntry.Size == 0)
        {
            return Result.Err<bool>(new InvalidOperationException("Invalid data offset or size"));
        }
        
        if (archiveEntry.DataOffset + archiveEntry.Size > inBuffer.Length)
        {
            return Result.Err<bool>(new InvalidOperationException("Invalid data offset or size"));
        }

        inBuffer.Seek(archiveEntry.DataOffset, SeekOrigin.Begin);
        inBuffer.CopyToLimit(outBuffer, (int) archiveEntry.Size);

        return Result.OkExn(true);
    }
    
    public Result<int, Exception> ExtractStreamToPath(Stream inStream, string outPath)
    {
        var fileEntriesOption = ParseFileEntries(inStream);
        if (!fileEntriesOption.IsSome(out var fileEntries))
        {
            return Result.Err<int>(new InvalidOperationException("Failed to parse file entries"));
        }

        if (!Directory.Exists(outPath))
        {
            Directory.CreateDirectory(outPath);
        }

        for (var i = 0; i < fileEntries.Length; i += 1)
        {
            var archiveEntry = fileEntries[i];
            if (archiveEntry.DataOffset == 0)
                continue;
            
            var directoryPath = Path.Join(outPath, Path.GetDirectoryName(archiveEntry.FilePath));
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
            
            var filePath = Path.Join(directoryPath, Path.GetFileName(archiveEntry.FilePath));
            using var outBuffer = new FileStream(filePath, FileMode.Create);

            var fileEntryResult = ReadFileEntry(inStream, archiveEntry, outBuffer);
            if (fileEntryResult.IsErr(out var ex))
            {
                return Result.Err<int>(ex ?? new InvalidOperationException("Failed to extract file entry"));
            }
        }

        return WriteEntryFile(outPath, fileEntries)
            .Map(b => b ? 0 : -1);
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

public static class SarcV02FileLibrary
{
    public const string XName = "sarc";
    public const int Version = 2;

    public static string VersionName = $"{XName.ToUpper()} v{Version:D2}";
}