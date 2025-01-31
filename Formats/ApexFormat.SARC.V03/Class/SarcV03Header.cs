using CommunityToolkit.HighPerformance;
using RustyOptions;

namespace ApexFormat.SARC.V03.Class;

/// <summary>
/// Structure:
/// <br/>MagicLength - <see cref="uint"/>
/// <br/>Magic - <see cref="uint"/>
/// <br/>Version - <see cref="uint"/>
/// <br/>EntrySize - <see cref="uint"/>
/// <br/>PathArraySize - <see cref="uint"/>
/// </summary>
public class SarcV03Header
{
    public uint MagicLength = SarcV03HeaderLibrary.MagicLength;
    public uint Magic = SarcV03HeaderLibrary.Magic;
    public uint Version = SarcV03HeaderLibrary.Version;
    public uint EntrySize = 0;
    public uint PathArraySize = 0;
}

public static class SarcV03HeaderLibrary
{
    public const int SizeOf = sizeof(uint) // MagicLength
                              + sizeof(uint) // Magic
                              + sizeof(uint) // Version
                              + sizeof(uint) // EntrySize
                              + sizeof(uint); // PathArraySize
    
    public const uint MagicLength = 0x04;
    public const uint Magic = 0x43524153; // "SARC"
    public const uint Version = 0x03;
    
    public static Option<SarcV03Header> ReadSarcV03Header(this Stream stream)
    {
        if (stream.Length - stream.Position < SizeOf)
        {
            return Option<SarcV03Header>.None;
        }
        
        var result = new SarcV03Header
        {
            MagicLength = stream.Read<uint>(),
            Magic = stream.Read<uint>(),
            Version = stream.Read<uint>(),
            EntrySize = stream.Read<uint>(),
            PathArraySize = stream.Read<uint>(),
        };

        if (result.MagicLength != MagicLength)
        {
            return Option<SarcV03Header>.None;
        }

        if (result.Magic != Magic)
        {
            return Option<SarcV03Header>.None;
        }

        if (result.Version != Version)
        {
            return Option<SarcV03Header>.None;
        }

        return Option.Some(result);
    }
}
