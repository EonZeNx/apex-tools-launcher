using ATL.Core.Class;
using ATL.Core.Extensions;
using ATL.Core.Hash;

namespace ApexFormat.TAB.V02;

public class TabV02Manager : ICanProcessStream, ICanProcessPath
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
        {
            return -1;
        }

        var archiveEntries = new List<TabV02Entry>();
        while (inTabBuffer.Position + TabV02Entry.SizeOf() <= inTabBuffer.Length)
        {
            var optionArchiveEntry = inTabBuffer.ReadTabV02Entry();
            if (optionArchiveEntry.IsSome(out var archiveEntry))
            {
                archiveEntries.Add(archiveEntry);
            }
        }
        outTabEntries = archiveEntries.ToArray();

        return 0;
    }

    public static int Decompress(Stream inTabBuffer, Stream inArcBuffer, string outDirectory)
    {
        if (!Directory.Exists(outDirectory))
        {
            return -1;
        }
        
        var optionHeader = inTabBuffer.ReadTabV02Header();
        if (optionHeader.IsNone)
        {
            return -2;
        }
        
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
            
            var hashLookupResult = HashDatabase.Lookup(tabEntry.NameHash, EHashType.FilePath);
            if (hashLookupResult.Valid())
            {
                filePath = Path.Join(outDirectory, hashLookupResult.Value);
            }

            if (!unknownDirectoryExists)
            { // cache result to reduce file system hit
                Directory.CreateDirectory(unknownDirectoryPath);
                unknownDirectoryExists = Directory.Exists(unknownDirectoryPath);
            }
            using var fileStream = new FileStream(filePath, FileMode.Create);
            
            inArcBuffer.Seek((int)tabEntry.Offset, SeekOrigin.Begin);
            inArcBuffer.CopyToLimit(fileStream, (int) tabEntry.Size);
        }
        
        return 0;
    }
}
