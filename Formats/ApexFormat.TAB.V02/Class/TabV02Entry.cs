using ApexToolsLauncher.Core.Extensions;
using ApexToolsLauncher.Core.Hash;
using CommunityToolkit.HighPerformance;
using RustyOptions;

namespace ApexFormat.TAB.V02.Class;

/// <summary>
/// Structure:
/// <br/>NameHash - <see cref="uint"/>
/// <br/>Offset - <see cref="uint"/>
/// <br/>Size - <see cref="uint"/>
/// </summary>
public class TabV02Entry
{
    public uint NameHash = 0;
    public uint Offset = 0;
    public uint Size = 0;
}

public static class TabV02EntryLibrary
{
    public const int SizeOf = sizeof(uint) // NameHash
                              + sizeof(uint) // Offset
                              + sizeof(uint); // Size

    public const string UnknownRelativePath = "__UNKNOWN";
    public static readonly Dictionary<string, Option<bool>> UnknownPathExists = new();
    
    public static Option<TabV02Entry> ReadTabV02Entry(this Stream stream)
    {
        if (!stream.CouldRead(SizeOf))
        {
            return Option<TabV02Entry>.None;
        }
        
        var result = new TabV02Entry
        {
            NameHash = stream.Read<uint>(),
            Offset = stream.Read<uint>(),
            Size = stream.Read<uint>(),
        };

        return Option.Some(result);
    }

    public static Option<Exception> ReadFromTabV02Arc(this TabV02Entry entry, Stream arcStream, string outPath)
    {
        if (entry.Offset >= arcStream.Length)
            return new Option<Exception>(new InvalidOperationException());
        
        arcStream.Seek((int) entry.Offset, SeekOrigin.Begin);
        if (!arcStream.CouldRead(entry.Size))
            return new Option<Exception>(new InvalidOperationException());
        
        var unknownDirectoryPath = Path.Join(outPath, UnknownRelativePath);
        var filePath = Path.Join(unknownDirectoryPath, $"{entry.NameHash:X8}");

        var optionHashResult = HashDatabases.Lookup(entry.NameHash, EHashType.FilePath);
        if (optionHashResult.IsSome(out var hashResult))
        {
            filePath = Path.Join(outPath, hashResult.Value);
                
            var fileDirectoryPath = Path.GetDirectoryName(filePath);
            if (fileDirectoryPath is not null && !Directory.Exists(fileDirectoryPath))
                Directory.CreateDirectory(fileDirectoryPath);
        }
        else
        {
            if (!UnknownPathExists.ContainsKey(unknownDirectoryPath))
            {
                Directory.CreateDirectory(unknownDirectoryPath);
                UnknownPathExists.Add(unknownDirectoryPath, Option.Some(Directory.Exists(unknownDirectoryPath)));
            }
        }
            
        using var fileStream = new FileStream(filePath, FileMode.Create);
            
        arcStream.Seek((int) entry.Offset, SeekOrigin.Begin);
        arcStream.CopyToLimit(fileStream, (int) entry.Size);

        return Option.None<Exception>();
    }
}