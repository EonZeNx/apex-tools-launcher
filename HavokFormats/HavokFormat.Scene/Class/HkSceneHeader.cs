using ATL.Core.Class;
using ATL.Core.Extensions;
using CommunityToolkit.HighPerformance;
using RustyOptions;

namespace HavokFormat.Scene.Class;

public static class HkSceneHeaderConstants
{
    public static readonly byte[] Magic = [0x57, 0xE0, 0xE0, 0x57, 0x10, 0xC0, 0xC0, 0x10, 0x00, 0x00, 0x00, 0x00];
    public const uint Version = 0x0B;
    public const int Extras0Length = 4;
    public const int Constants0Length = 20;
    public const int VersionNameLength = 14;
    public const int Constants1Length = 6;
    public const int Extras1Length = 2;
    public const int Pad0Length = 2;
}

public class HkSceneHeader : ISizeOf
{
    public byte[] Magic = HkSceneHeaderConstants.Magic;
    public uint Version = HkSceneHeaderConstants.Version;
    public byte[] Extras0 = new byte[HkSceneHeaderConstants.Extras0Length];
    public byte[] Constants0 = new byte[HkSceneHeaderConstants.Constants0Length];
    public string VersionName = "";
    public byte[] Constants1 = new byte[HkSceneHeaderConstants.Constants1Length];
    public byte[] Extras1 = new byte[HkSceneHeaderConstants.Extras1Length];
    public byte[] Pad0 = new byte[HkSceneHeaderConstants.Pad0Length];

    public static uint SizeOf()
    {
        return (uint) (HkSceneHeaderConstants.Magic.Length + // Magic
                      sizeof(uint) + // Version
                      HkSceneHeaderConstants.Extras0Length + // Extras0
                      HkSceneHeaderConstants.Constants0Length + // Constants0
                      HkSceneHeaderConstants.VersionNameLength + // VersionName
                      HkSceneHeaderConstants.Constants1Length + // Constants1
                      HkSceneHeaderConstants.Extras1Length + // Extras1
                      HkSceneHeaderConstants.Pad0Length); // Pad0
    }
}

public static class HkSceneHeaderExtensions
{
    public static Option<HkSceneHeader> ReadHkSceneHeader(this Stream stream)
    {
        if (stream.Length - stream.Position < HkSceneHeader.SizeOf())
        {
            return Option<HkSceneHeader>.None;
        }
        
        var result = new HkSceneHeader
        {
            Magic = stream.ReadArray<byte>(HkSceneHeaderConstants.Magic.Length),
            Version = stream.Read<uint>(),
            Extras0 = stream.ReadArray<byte>(HkSceneHeaderConstants.Extras0Length),
            Constants0 = stream.ReadArray<byte>(HkSceneHeaderConstants.Constants0Length),
            VersionName = stream.ReadStringOfLength(HkSceneHeaderConstants.VersionNameLength),
            Constants1 = stream.ReadArray<byte>(HkSceneHeaderConstants.Constants1Length),
            Extras1 = stream.ReadArray<byte>(HkSceneHeaderConstants.Extras1Length),
            Pad0 = stream.ReadArray<byte>(HkSceneHeaderConstants.Pad0Length),
        };

        if (!result.Magic.SequenceEqual(HkSceneHeaderConstants.Magic))
        {
            return Option<HkSceneHeader>.None;
        }

        if (result.Version != HkSceneHeaderConstants.Version)
        {
            return Option<HkSceneHeader>.None;
        }

        return Option.Some(result);
    }
}