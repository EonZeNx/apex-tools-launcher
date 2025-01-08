using ApexToolsLauncher.Core.Class;
using ApexToolsLauncher.Core.Extensions;
using CommunityToolkit.HighPerformance;
using RustyOptions;

namespace ApexFormat.RTPC.V01.Class;

/// <summary>
/// Structure:
/// <br/>Magic - <see cref="uint"/>
/// <br/>MajorVersion - <see cref="ushort"/>
/// <br/>MinorVersion - <see cref="ushort"/>
/// </summary>
public class RtpcV01Header : ISizeOf
{
    public uint Magic = RtpcV01HeaderLibrary.Magic;
    public ushort MajorVersion = RtpcV01HeaderLibrary.MajorVersion;
    public ushort MinorVersion = RtpcV01HeaderLibrary.MinorVersion;

    public static uint SizeOf()
    {
        return sizeof(uint) + // Magic
               sizeof(ushort) + // MajorVersion
               sizeof(ushort); // MinorVersion
    }
}

public static class RtpcV01HeaderLibrary
{
    public const int SizeOf = sizeof(uint) // Magic
                              + sizeof(ushort) // MajorVersion
                              + sizeof(ushort); // MinorVersion
    
    public const uint Magic = 0x43505452; // RTPC
    public const ushort MajorVersion = 0x01;
    public const ushort MinorVersion = 0x00;
    
    public static Option<RtpcV01Header> ReadRtpcV01Header(this Stream stream)
    {
        if (stream.CouldRead(SizeOf))
        {
            return Option<RtpcV01Header>.None;
        }
        
        var result = new RtpcV01Header
        {
            Magic = stream.Read<uint>(),
            MajorVersion = stream.Read<ushort>(),
            MinorVersion = stream.Read<ushort>(),
        };

        if (result.Magic != Magic)
        {
            return Option<RtpcV01Header>.None;
        }

        if (result.MajorVersion != MajorVersion)
        {
            return Option<RtpcV01Header>.None;
        }

        if (result.MinorVersion != MinorVersion)
        {
            return Option<RtpcV01Header>.None;
        }

        return Option.Some(result);
    }
}