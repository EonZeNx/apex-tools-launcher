namespace ApexFormat.AVTX.V01.Class;

public static class AvtxV01StreamConstants
{
    public const ushort Alignment = 16;
}

public class AvtxV01Stream
{
    public uint Offset      = 0;
    public uint Size        = 0;
    public ushort Alignment = AvtxV01StreamConstants.Alignment;
    public bool TileMode    = false;
    public byte Source      = 0;
}