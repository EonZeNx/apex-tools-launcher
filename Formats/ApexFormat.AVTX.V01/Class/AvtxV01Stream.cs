using ApexToolsLauncher.Core.Class;
using CommunityToolkit.HighPerformance;
using RustyOptions;

namespace ApexFormat.AVTX.V01.Class;

public static class AvtxV01StreamConstants
{
    public const ushort Alignment = 16;
}

public class AvtxV01Stream : ISizeOf
{
    public uint Offset      = 0;
    public uint Size        = 0;
    public ushort Alignment = AvtxV01StreamConstants.Alignment;
    public bool TileMode    = false;
    public byte Source      = 0;

    public static uint SizeOf()
    {
        return sizeof(uint) + // Offset
               sizeof(uint) + // Size
               sizeof(ushort) + // Alignment
               sizeof(bool) + // TileMode
               sizeof(byte); // Source
    }
}

public static class AvtxV01StreamExtensions
{
    public static Option<AvtxV01Stream> ReadAvtxV01Stream(this Stream stream)
    {
        if (stream.Length - stream.Position < AvtxV01Stream.SizeOf())
        {
            return Option<AvtxV01Stream>.None;
        }

        var result = new AvtxV01Stream
        {
            Offset = stream.Read<uint>(),
            Size = stream.Read<uint>(),
            Alignment = stream.Read<ushort>(),
            TileMode = stream.Read<bool>(),
            Source = stream.Read<byte>()
        };

        return Option.Some(result);
    }
}