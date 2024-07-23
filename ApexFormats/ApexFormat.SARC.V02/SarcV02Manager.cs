using System.Xml;
using System.Xml.Linq;
using ATL.Core.Class;
using ATL.Core.Libraries;

namespace ApexFormat.SARC.V02;

public class SarcV02Manager : ICanProcessStream, ICanProcessPath, IProcessBasic
{
    public static bool CanProcess(Stream stream)
    {
        return !stream.ReadSarcV02Header().IsNone;
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

    public static int ParseFileEntries(Stream inBuffer, out SarcV02ArchiveEntry[] outEntries)
    {
        outEntries = [];
        if (inBuffer.Length == 0)
            return -1;

        var optionHeader = inBuffer.ReadSarcV02Header();
        if (!optionHeader.IsSome(out var header))
            return -2;
        
        var archiveEntries = new List<SarcV02ArchiveEntry>();

        var startPosition = inBuffer.Position;
        while (true)
        {
            var optionArchiveEntry = inBuffer.ReadSarcV02ArchiveEntry();
            if (!optionArchiveEntry.IsSome(out var archiveEntry))
                continue;
            
            archiveEntries.Add(archiveEntry);
            if (header.Size - (inBuffer.Position - startPosition) <= 15)
            {
                break;
            }
        }

        outEntries = archiveEntries.ToArray();

        return 0;
    }

    public static int ReadFileEntry(Stream inBuffer, SarcV02ArchiveEntry archiveEntry, Stream outBuffer)
    {
        if (archiveEntry.DataOffset == 0 || archiveEntry.Size == 0)
        {
            return 0;
        }
        if (archiveEntry.DataOffset + archiveEntry.Size > inBuffer.Length)
        {
            return -1;
        }

        inBuffer.Seek(archiveEntry.DataOffset, SeekOrigin.Begin);
        inBuffer.CopyTo(outBuffer, (int) archiveEntry.Size);

        return 0;
    }

    public static int WriteEntryFile(string outDirectory, SarcV02ArchiveEntry[] entries)
    {
        var outer = new XElement("archive");
        outer.SetAttributeValue("extension", "ee");
        outer.SetAttributeValue("format", "SARC");
        outer.SetAttributeValue("version", "2");
        
        var root = new XElement("files");
        foreach (var archiveEntry in entries)
        {
            var entry = new XElement("file");
            entry.SetAttributeValue("size", archiveEntry.Size);
            entry.SetAttributeValue("ref", archiveEntry.DataOffset == 0);
            entry.SetValue(archiveEntry.FilePath);
            
            root.Add(entry);
        }
        outer.Add(root);

        var xmlFilePath = Path.Join(outDirectory, "@files.xml");
        using var xmlFile = new FileStream(xmlFilePath, FileMode.Create);
        
        var xd = new XDocument(XDocumentLibrary.AtlGeneratedComment(), outer);
        using var xw = XmlWriter.Create(xmlFile, new XmlWriterSettings
        {
            Indent = true,
            IndentChars = "\t"
        });
        xd.Save(xw);

        return 0;
    }
    
    public static int Decompress(Stream inBuffer, string outDirectory)
    {
        var parseFileEntriesResult = ParseFileEntries(inBuffer, out var outEntries);
        if (parseFileEntriesResult < 0)
        {
            return parseFileEntriesResult;
        }

        if (!Directory.Exists(outDirectory))
        {
            Directory.CreateDirectory(outDirectory);
        }

        for (var i = 0; i < outEntries.Length; i += 1)
        {
            var archiveEntry = outEntries[i];
            
            var directoryPath = Path.Join(outDirectory, Path.GetDirectoryName(archiveEntry.FilePath));
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
            
            var filePath = Path.Join(directoryPath, Path.GetFileName(archiveEntry.FilePath));
            var outBuffer = new FileStream(filePath, FileMode.Create);
            var archiveEntryResult = ReadFileEntry(inBuffer, archiveEntry, outBuffer);

            if (archiveEntryResult < 0)
            {
                return archiveEntryResult;
            }
        }
        
        return WriteEntryFile(outDirectory, outEntries);
    }
    
    public int ProcessBasic(string inFilePath, string outDirectory)
    {
        var inBuffer = new FileStream(inFilePath, FileMode.Open);
        
        var outDirectoryPath = Path.GetDirectoryName(inFilePath);
        if (!string.IsNullOrEmpty(outDirectory) && Directory.Exists(outDirectory))
            outDirectoryPath = outDirectory;
        
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(inFilePath);
        var directoryPath = Path.Join(outDirectoryPath, fileNameWithoutExtension);
            
        if (!Directory.Exists(directoryPath))
            Directory.CreateDirectory(directoryPath);

        var result = Decompress(inBuffer, directoryPath);
        return result;
    }
}
