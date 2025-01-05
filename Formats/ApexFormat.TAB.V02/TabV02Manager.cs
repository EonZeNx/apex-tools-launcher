using ApexToolsLauncher.Core.Class;
using ApexToolsLauncher.Core.Extensions;
using ApexToolsLauncher.Core.Hash;

namespace ApexFormat.TAB.V02;

public class TabV02Manager : ICanProcessStream, ICanProcessPath, IProcessBasic
{
    public static bool CanProcess(Stream stream)
    {
        return !stream.ReadTabV02Header().IsNone;
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
    
    public static int ParseTabEntries(Stream inTabBuffer, out TabV02Entry[] outTabEntries)
    {
        outTabEntries = [];
        if (inTabBuffer.Length == 0)
            return -1;

        var archiveEntries = new List<TabV02Entry>();
        while (inTabBuffer.Position < inTabBuffer.Length)
        {
            var optionArchiveEntry = inTabBuffer.ReadTabV02Entry();
            if (!optionArchiveEntry.IsSome(out var archiveEntry))
                continue;
            
            archiveEntries.Add(archiveEntry);
        }
        outTabEntries = archiveEntries.ToArray();

        return 0;
    }

    public static int Decompress(Stream inTabBuffer, Stream inArcBuffer, string outDirectory)
    {
        if (!Directory.Exists(outDirectory))
            return -1;
        
        var optionHeader = inTabBuffer.ReadTabV02Header();
        if (optionHeader.IsNone)
            return -2;
        
        var parseFileEntriesResult = ParseTabEntries(inTabBuffer, out var tabEntries);
        if (parseFileEntriesResult < 0)
        {
            return parseFileEntriesResult;
        }

        var unknownDirectoryPath = Path.Join(outDirectory, "__UNKNOWN");
        var unknownDirectoryExists = Directory.Exists(unknownDirectoryPath);
        foreach (var tabEntry in tabEntries)
        {
            var filePath = Path.Join(unknownDirectoryPath, $"{tabEntry.NameHash:X8}");

            var optionHashResult = HashDatabases.Lookup(tabEntry.NameHash, EHashType.FilePath);
            if (optionHashResult.IsSome(out var hashResult))
            {
                filePath = Path.Join(outDirectory, hashResult.Value);
                
                var fileDirectoryPath = Path.GetDirectoryName(filePath);
                if (fileDirectoryPath is not null && !Directory.Exists(fileDirectoryPath))
                    Directory.CreateDirectory(fileDirectoryPath);
            }
            else
            {
                if (!unknownDirectoryExists)
                { // cache result to reduce file system hit
                    Directory.CreateDirectory(unknownDirectoryPath);
                    unknownDirectoryExists = Directory.Exists(unknownDirectoryPath);
                }
            }
            
            using var fileStream = new FileStream(filePath, FileMode.Create);
            
            inArcBuffer.Seek((int) tabEntry.Offset, SeekOrigin.Begin);
            inArcBuffer.CopyToLimit(fileStream, (int) tabEntry.Size);
        }
        
        return 0;
    }
    
    public int ProcessBasic(string inFilePath, string outDirectory)
    {
        using var tabBuffer = new FileStream(inFilePath, FileMode.Open);
        
        var directoryPath = Path.GetDirectoryName(inFilePath);
        if (!Directory.Exists(directoryPath))
            return -1;
        
        if (string.IsNullOrEmpty(outDirectory) || !Directory.Exists(outDirectory))
            outDirectory = directoryPath;
        
        var fileNameWoExtension = Path.GetFileNameWithoutExtension(inFilePath);
        var arcPath = Path.Join(directoryPath, $"{fileNameWoExtension}.arc");
        using var arcBuffer = new FileStream(arcPath, FileMode.Open);
        
        var result = Decompress(tabBuffer, arcBuffer, outDirectory);
        return result;
    }

    public string GetProcessorName()
    {
        return "TAB v02";
    }
}
