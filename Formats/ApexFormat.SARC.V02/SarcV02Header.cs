using ApexToolsLauncher.Core.Class;
using CommunityToolkit.HighPerformance;
using RustyOptions;

namespace ApexFormat.SARC.V02;

public static class SarcV02HeaderConstants
{
    public const uint MagicLength = 0x04;
    public const uint Magic = 0x43524153; // "SARC"
    public const uint Version = 0x02;
}

/// <summary>
/// Structure:
/// <br/>MagicLength - <see cref="uint"/>
/// <br/>Magic - <see cref="uint"/>
/// <br/>Version - <see cref="uint"/>
/// <br/>Size - <see cref="uint"/>
/// </summary>
public class SarcV02Header : ISizeOf
{
    public uint MagicLength = SarcV02HeaderConstants.MagicLength;
    public uint Magic = SarcV02HeaderConstants.Magic;
    public uint Version = SarcV02HeaderConstants.Version;
    public uint Size;

    public static uint SizeOf()
    {
        return sizeof(uint) + // MagicLength
               sizeof(uint) + // Magic
               sizeof(uint) + // Version
               sizeof(uint); // Size
    }
}

public static class SarcV02HeaderExtensions
{
    public static Option<SarcV02Header> ReadSarcV02Header(this Stream stream)
    {
        if (stream.Length - stream.Position < SarcV02Header.SizeOf())
        {
            return Option<SarcV02Header>.None;
        }
        
        var result = new SarcV02Header
        {
            MagicLength = stream.Read<uint>(),
            Magic = stream.Read<uint>(),
            Version = stream.Read<uint>(),
            Size = stream.Read<uint>()
        };

        if (result.MagicLength != SarcV02HeaderConstants.MagicLength)
        {
            return Option<SarcV02Header>.None;
        }

        if (result.Magic != SarcV02HeaderConstants.Magic)
        {
            return Option<SarcV02Header>.None;
        }

        if (result.Version != SarcV02HeaderConstants.Version)
        {
            return Option<SarcV02Header>.None;
        }

        return Option.Some(result);
    }
}