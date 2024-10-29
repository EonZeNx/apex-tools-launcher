using ApexFormat.ADF.V04.Enums;
using ATL.Core.Class;
using ATL.Core.Extensions;
using CommunityToolkit.HighPerformance;
using RustyOptions;

namespace ApexFormat.ADF.V04.Class;

public static class AdfV04HeaderConstants
{
    public const uint Magic = 0x41444620; // "ADF "
    public const uint Version = 0x04;
    public const uint Unknown01 = 32;
    public const uint Unknown02 = 32;
}

/// <summary>
/// Structure:
/// <br/>Magic - <see cref="uint"/>
/// <br/>Version - <see cref="uint"/>
/// <br/>InstanceCount - <see cref="string"/>
/// <br/>InstanceOffset - <see cref="uint"/>
/// <br/>TypeCount - <see cref="uint"/>
/// <br/>TypeOffset - <see cref="uint"/>
/// <br/>StringHashCount - <see cref="uint"/>
/// <br/>StringHashOffset - <see cref="uint"/>
/// <br/>StringTableCount - <see cref="uint"/>
/// <br/>StringTableOffset - <see cref="uint"/>
/// <br/>FileSize - <see cref="uint"/>
/// <br/>MetaDataOffset - <see cref="uint"/>
/// <br/>Flags - <see cref="EAdfV04HeaderFlags"/>
/// <br/>IncludedLibraries - <see cref="uint"/>
/// <br/>Unknown01 - <see cref="uint"/>
/// <br/>Unknown02 - <see cref="uint"/>
/// <br/>Comment - <see cref="string"/>
/// </summary>
public class AdfV04Header : ISizeOf
{
    public uint Magic = AdfV04HeaderConstants.Magic;
    public uint Version = AdfV04HeaderConstants.Version;
    public uint InstanceCount = 0;
    public uint InstanceOffset = 0;
    public uint TypeCount = 0;
    public uint TypeOffset = 0;
    public uint StringHashCount = 0;
    public uint StringHashOffset = 0;
    public uint StringTableCount = 0;
    public uint StringTableOffset = 0;
    public uint FileSize = 0;
    public uint MetaDataOffset = 0;
    public EAdfV04HeaderFlags Flags = EAdfV04HeaderFlags.Default;
    public uint IncludedLibraries = 0;
    public uint Unknown01 = AdfV04HeaderConstants.Unknown01;
    public uint Unknown02 = AdfV04HeaderConstants.Unknown02;
    public string Comment = string.Empty;

    public static uint SizeOf()
    {
        return sizeof(uint) + // Magic
               sizeof(uint) + // Version
               sizeof(uint) + // InstanceCount
               sizeof(uint) + // FirstInstanceOffset
               sizeof(uint) + // TypeCount
               sizeof(uint) + // TypeOffset
               sizeof(uint) + // StringHashCount
               sizeof(uint) + // StringHashOffset
               sizeof(uint) + // StringTableCount
               sizeof(uint) + // StringTableOffset
               sizeof(uint) + // FileSize
               sizeof(uint) + // MetaDataOffset
               sizeof(EAdfV04HeaderFlags) + // Flags
               sizeof(uint) + // IncludedLibraries
               sizeof(uint) + // Unknown01
               sizeof(uint) + // Unknown02
               sizeof(byte); // Min string length
    }
}

public static class AdfV04HeaderExtensions
{
    public static Option<AdfV04Header> ReadAdfV04Header(this Stream stream)
    {
        if (stream.Length - stream.Position < AdfV04Header.SizeOf())
        {
            return Option<AdfV04Header>.None;
        }

        var result = new AdfV04Header
        {
            Magic = stream.Read<uint>(),
            Version = stream.Read<uint>(),
            InstanceCount = stream.Read<uint>(),
            InstanceOffset = stream.Read<uint>(),
            TypeCount = stream.Read<uint>(),
            TypeOffset = stream.Read<uint>(),
            StringHashCount = stream.Read<uint>(),
            StringHashOffset = stream.Read<uint>(),
            StringTableCount = stream.Read<uint>(),
            StringTableOffset = stream.Read<uint>(),
            FileSize = stream.Read<uint>(),
            MetaDataOffset = stream.Read<uint>(),
            Flags = stream.Read<EAdfV04HeaderFlags>(),
            IncludedLibraries = stream.Read<uint>(),
            Unknown01 = stream.Read<uint>(),
            Unknown02 = stream.Read<uint>(),
            Comment = stream.ReadStringZ(),
        };
        
        if (result.Magic != AdfV04HeaderConstants.Magic)
        {
            return Option<AdfV04Header>.None;
        }

        if (result.Version != AdfV04HeaderConstants.Version)
        {
            return Option<AdfV04Header>.None;
        }

        return Option.Some(result);
    }
}