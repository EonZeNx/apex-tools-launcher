using ApexToolsLauncher.Core.Class;
using ApexToolsLauncher.Core.Extensions;
using CommunityToolkit.HighPerformance;
using RustyOptions;

namespace HavokFormat.Scene.Class;

public static class HkClassNameConstants
{
    public const byte SupportedUnk0 = 0x09;
}

public class HkClassName : ISizeOf
{
    public uint CompressedUuid = 0;
    public byte Unk0 = 0;
    public string Name = "";

    public static uint SizeOf()
    {
        return sizeof(uint) + // CompressedUuid
               sizeof(byte) + // Unk0
               sizeof(byte); // Min string length
    }

    public override string ToString()
    {
        return $"{Name}: {CompressedUuid}";
    }
}

public static class HkClassNameExtensions
{
    public static Option<HkClassName> ReadHkClassName(this Stream stream)
    {
        if (stream.Length - stream.Position < HkSceneHeader.SizeOf())
        {
            return Option<HkClassName>.None;
        }
        
        var result = new HkClassName
        {
            CompressedUuid = stream.Read<uint>(),
            Unk0 = stream.Read<byte>()
        };
        
        if (result.Unk0 != HkClassNameConstants.SupportedUnk0)
            return Option<HkClassName>.None;

        result.Name = stream.ReadStringZ();

        return Option.Some(result);
    }
}