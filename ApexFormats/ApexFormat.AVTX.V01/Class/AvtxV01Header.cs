namespace ApexFormat.AVTX.V01.Class;

public static class AvtxV01HeaderConstants
{
    public const uint Magic      = 0x58545641; // "XTVA"
    public const byte Version    = 0x01;
    public const byte Dimension  = 0x02;
    public const ushort Depth    = 0x01;
    public const uint MaxStreams = 0x08;
}

public class AvtxV01Header
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
}