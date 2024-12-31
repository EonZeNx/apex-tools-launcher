﻿using ApexToolsLauncher.Core.Class;
using ApexToolsLauncher.Core.Extensions;
using CommunityToolkit.HighPerformance;
using RustyOptions;

namespace ApexFormat.SARC.V02;

public static class SarcV02ArchiveEntryConstants
{
    public const uint FilePathAlignment = 0x04;
}

/// <summary>
/// Structure:
/// <br/>FilePath Length - <see cref="uint"/>
/// <br/>FilePath - <see cref="string"/>
/// <br/>DataOffset - <see cref="uint"/>
/// <br/>Size - <see cref="uint"/>
/// </summary>
public class SarcV02ArchiveEntry : ISizeOf
{
    public string FilePath = ""; // length prefix, min size = uint
    public uint DataOffset = 0;
    public uint Size = 0;

    public static uint SizeOf()
    {
        return sizeof(uint) +
               sizeof(uint) +
               sizeof(uint);
    }
}

public static class SarcV02ArchiveEntryExtensions
{
    public static Option<SarcV02ArchiveEntry> ReadSarcV02ArchiveEntry(this Stream stream)
    {
        if (stream.Length - stream.Position < SarcV02ArchiveEntry.SizeOf())
        {
            return Option<SarcV02ArchiveEntry>.None;
        }
        
        var result = new SarcV02ArchiveEntry
        {
            FilePath = stream.ReadStringLengthPrefix().Replace("\0", ""),
            DataOffset = stream.Read<uint>(),
            Size = stream.Read<uint>()
        };

        return Option.Some(result);
    }
}