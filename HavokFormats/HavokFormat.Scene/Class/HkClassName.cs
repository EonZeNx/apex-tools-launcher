using ATL.Core.Class;
using ATL.Core.Extensions;
using CommunityToolkit.HighPerformance;
using RustyOptions;

namespace HavokFormat.Scene.Class;

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
            Unk0 = stream.Read<byte>(),
            Name = stream.ReadStringZ(),
        };

        return Option.Some(result);
    }
}