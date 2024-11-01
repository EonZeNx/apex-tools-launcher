using ApexToolsLauncher.Core.Class;
using CommunityToolkit.HighPerformance;
using RustyOptions;

namespace ApexFormat.AVTX.V01.Class;

public static class AvtxV01HeaderConstants
{
    public const uint Magic      = 0x58545641; // "XTVA"
    public const byte Version    = 0x01;
    public const byte Dimension  = 0x02;
    public const ushort Depth    = 0x01;
    public const uint MaxStreams = 0x08;
}

public class AvtxV01Header : ISizeOf
{
    public uint Magic         = AvtxV01HeaderConstants.Magic;
    public byte Version       = AvtxV01HeaderConstants.Version;
    public byte Platform      = 0;
    public byte Tag           = 0;
    public byte Dimension     = AvtxV01HeaderConstants.Dimension;
    public uint Format        = 0;
    public ushort Width       = 0;
    public ushort Height      = 0;
    public ushort Depth       = AvtxV01HeaderConstants.Depth;
    public ushort Flags       = 0;
    public byte Mips          = 0;
    public byte HeaderMips    = 0;
    public byte CinematicMips = 0;
    public byte MipsBias      = 0;
    public byte LodGroup      = 0;
    public byte Pool          = 0;
    public byte Reserved01    = 0;
    public byte Reserved02    = 0;
    public uint Reserved03    = 0;
    
    public AvtxV01Stream[] Streams = new AvtxV01Stream[AvtxV01HeaderConstants.MaxStreams];

    public static uint SizeOf()
    {
        return sizeof(uint) + // Magic
               sizeof(byte) + // Version
               sizeof(byte) + // Platform
               sizeof(byte) + // Tag
               sizeof(byte) + // Dimension
               sizeof(uint) + // Format
               sizeof(ushort) + // Width
               sizeof(ushort) + // Height
               sizeof(ushort) + // Depth
               sizeof(ushort) + // Flags
               sizeof(byte) + // Mips
               sizeof(byte) + // HeaderMips
               sizeof(byte) + // CinematicMips
               sizeof(byte) + // MipsBias
               sizeof(byte) + // LodGroup
               sizeof(byte) + // Pool
               sizeof(byte) + // Reserved01
               sizeof(byte) + // Reserved02
               sizeof(uint); // Reserved03
    }
}

public static class AvtxV01HeaderExtensions
{
    public static Option<AvtxV01Header> ReadAvtxV01Header(this Stream stream)
    {
        if (stream.Length - stream.Position < AvtxV01Header.SizeOf())
        {
            return Option<AvtxV01Header>.None;
        }

        var result = new AvtxV01Header
        {
            Magic = stream.Read<uint>(),
            Version = stream.Read<byte>(),
            Platform = stream.Read<byte>(),
            Tag = stream.Read<byte>(),
            Dimension = stream.Read<byte>(),
            Format = stream.Read<uint>(),
            Width = stream.Read<ushort>(),
            Height = stream.Read<ushort>(),
            Depth = stream.Read<ushort>(),
            Flags = stream.Read<ushort>(),
            Mips = stream.Read<byte>(),
            HeaderMips = stream.Read<byte>(),
            CinematicMips = stream.Read<byte>(),
            MipsBias = stream.Read<byte>(),
            LodGroup = stream.Read<byte>(),
            Pool = stream.Read<byte>(),
            Reserved01 = stream.Read<byte>(),
            Reserved02 = stream.Read<byte>(),
            Reserved03 = stream.Read<uint>()
        };

        for (var i = 0; i < result.Streams.Length; i++)
        {
            var optionStream = stream.ReadAvtxV01Stream();
            if (!optionStream.IsSome(out var avtxStream))
                break;

            result.Streams[i] = avtxStream;
        }
        
        if (result.Magic != AvtxV01HeaderConstants.Magic)
        {
            return Option<AvtxV01Header>.None;
        }

        if (result.Version != AvtxV01HeaderConstants.Version)
        {
            return Option<AvtxV01Header>.None;
        }

        return Option.Some(result);
    }
    
    public static byte FindBestStream(this AvtxV01Header header, byte targetSource)
    {
        uint biggest = 0;
        var streamIndex = 0;
        
        for (var i = 0; i < header.Streams.Length; i++)
        {
            var avtxStream = header.Streams[i];
            if (avtxStream.Size == 0)
                continue;

            if (avtxStream.Source != targetSource && (avtxStream.Source != 0 || avtxStream.Size <= biggest))
                continue;
            
            biggest = avtxStream.Size;
            streamIndex = i;
        }

        return (byte) streamIndex;
    }

    public static uint GetRank(this AvtxV01Header header, byte index)
    {
        return (uint) (header.Mips - (header.HeaderMips + index));
    }
    
    public static Option<AvtxV01TextureEntry> GetBestEntry(this AvtxV01Header header)
    {
        var streamIndex = header.FindBestStream(0);
        var rank = header.GetRank(streamIndex);
        var avtxStream = header.Streams[rank];

        var result = new AvtxV01TextureEntry
        {
            Width = (ushort) (header.Width >> (ushort) rank),
            Height = (ushort) (header.Height >> (ushort) rank),
            Depth = (ushort) (header.Depth >> (ushort) rank),
            Format = header.Format,
            Source = avtxStream.Source
        };

        return Option.Some(result);
    }
}