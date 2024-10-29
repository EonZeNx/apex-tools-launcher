using ATL.Core.Class;
using ATL.Core.Extensions;
using CommunityToolkit.HighPerformance;
using RustyOptions;

namespace HavokFormat.Scene.Class;

public static class HkSceneSectionConstants
{
    public const int NameLength = 16;
    public const int Constants0Length = 4;
    public const int Pad0Length = 16;
}

public class HkSceneSection : ISizeOf
{
    public string Name = "";
    public byte[] Constants0 = new byte[HkSceneSectionConstants.Constants0Length];
    public uint Offset = 0;
    public uint Data1Offset = 0;
    public uint Data2Offset = 0;
    public uint Data3Offset = 0;
    public uint Data4Offset = 0;
    public uint Data5Offset = 0;
    public uint DataEndOffset = 0;
    public byte[] Pad0 = new byte[HkSceneSectionConstants.Pad0Length];

    public static uint SizeOf()
    {
        return HkSceneSectionConstants.NameLength + // Name
               HkSceneSectionConstants.Constants0Length + // Constants0
               sizeof(uint) + // Offset
               sizeof(uint) + // Data1Offset
               sizeof(uint) + // Data2Offset
               sizeof(uint) + // Data3Offset
               sizeof(uint) + // Data4Offset
               sizeof(uint) + // Data5Offset
               sizeof(uint) + // DataEndOffset
               HkSceneSectionConstants.Pad0Length; // Pad0
    }

    public override string ToString()
    {
        return $"{Name} from {Offset} to {DataEndOffset}";
    }
}

public static class HkSceneSectionExtensions
{
    public static Option<HkSceneSection> ReadHkSceneSection(this Stream stream)
    {
        if (stream.Length - stream.Position < HkSceneHeader.SizeOf())
        {
            return Option<HkSceneSection>.None;
        }
        
        var result = new HkSceneSection
        {
            Name = stream.ReadStringOfLength(HkSceneSectionConstants.NameLength),
            Constants0 = stream.ReadArray<byte>(HkSceneSectionConstants.Constants0Length),
            Offset = stream.Read<uint>(),
            Data1Offset = stream.Read<uint>(),
            Data2Offset = stream.Read<uint>(),
            Data3Offset = stream.Read<uint>(),
            Data4Offset = stream.Read<uint>(),
            Data5Offset = stream.Read<uint>(),
            DataEndOffset = stream.Read<uint>(),
            Pad0 = stream.ReadArray<byte>(HkSceneSectionConstants.Pad0Length),
        };

        return Option.Some(result);
    }
}