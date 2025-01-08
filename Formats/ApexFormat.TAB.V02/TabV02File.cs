using ApexFormat.TAB.V02.Class;
using ApexToolsLauncher.Core.Class;
using RustyOptions;

namespace ApexFormat.TAB.V02;

public class TabV02File : ICanExtractPath, IExtractPathToPath, ICanRepackPath, IRepackPathToPath
{
    public bool CanExtractPath(string path)
    {
        if (!File.Exists(path))
            return false;
        
        using var fileStream = new FileStream(path, FileMode.Open);

        var result = false;
        try
        {
            result = !fileStream.ReadTabV02Header().IsNone;
        }
        catch (Exception)
        {
        }

        return result;
    }
    
    public static Result<TabV02Entry[], Exception> ParseTabEntries(Stream inStream)
    {
        if (inStream.Length == 0)
            return Result.Err<TabV02Entry[]>(new EndOfStreamException());

        var archiveEntries = new List<TabV02Entry>();
        while (inStream.Position < inStream.Length)
        {
            var optionArchiveEntry = inStream.ReadTabV02Entry();
            if (!optionArchiveEntry.IsSome(out var archiveEntry))
                continue;
            
            archiveEntries.Add(archiveEntry);
        }

        return Result.OkExn(archiveEntries.ToArray());
    }

    public Result<int, Exception> ExtractPathToPath(string inPath, string outPath)
    {
        using var tabStream = new FileStream(inPath, FileMode.Open);
        
        var directoryPath = Path.GetDirectoryName(inPath);
        if (!Directory.Exists(directoryPath))
            return Result.Err<int>(new DirectoryNotFoundException($"Failed to find {directoryPath}"));

        var outDirectory = outPath;
        if (string.IsNullOrEmpty(outPath) || !Directory.Exists(outPath))
            outDirectory = directoryPath;
        
        var fileNameWoExtension = Path.GetFileNameWithoutExtension(inPath);
        var arcPath = Path.Join(directoryPath, $"{fileNameWoExtension}.arc");
        using var arcStream = new FileStream(arcPath, FileMode.Open);
        
        var optionHeader = tabStream.ReadTabV02Header();
        if (optionHeader.IsNone)
            return Result.Err<int>(new InvalidOperationException($"Failed to read header"));
        
        var tabEntriesResult = ParseTabEntries(tabStream);
        if (tabEntriesResult.Err().IsSome(out var err))
        {
            return Result.Err<int>(err);
        }

        var tabEntries = tabEntriesResult.Unwrap();

        foreach (var tabEntry in tabEntries)
        {
            var arcResult = tabEntry.ReadFromTabV02Arc(arcStream, outDirectory);
            if (arcResult.IsSome(out var e))
                return Result.Err<int>(e);
        }
        
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
}

public static class TabV02FileLibrary
{
    public const int Version = 2;
}