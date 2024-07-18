using ATL.Core.Extensions;

namespace ApexFormat.TAB.V02;

public static class TabV02Manager
{
    public static bool SupportsPath(string path)
    {
        if (Path.Exists(path))
        { // invalid path
            return false;
        }
        
        if (Directory.Exists(path))
        { // don't support repacking directories just yet
            return false;
        }
        
        if (!File.Exists(path))
        { // check if file valid
            using var fileStream = new FileStream(path, FileMode.Open);
            return !fileStream.ReadTabV02Header().IsNone;
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
        var optionHeader = inTabBuffer.ReadTabV02Header();
        var parseFileEntriesResult = ParseTabEntries(inTabBuffer, out var tabEntries);
        if (parseFileEntriesResult < 0)
        {
            return parseFileEntriesResult;
        }

        if (!Directory.Exists(outDirectory))
        {
            return -1;
        }

        var unknownDirectoryPath = Path.Join(outDirectory, "__UNKNOWN");
        var unknownDirectoryExists = Directory.Exists(unknownDirectoryPath);
        foreach (var tabEntry in tabEntries)
        {
            // TODO: Lookup hash
            var filePath = Path.Join(unknownDirectoryPath, $"{tabEntry.NameHash:X8}");

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
