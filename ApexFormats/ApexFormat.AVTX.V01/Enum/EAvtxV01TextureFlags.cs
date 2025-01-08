namespace ApexFormat.AVTX.V01.Enum;

[Flags]
public enum EAvtxV01TextureFlags : uint
{
    Streamed      = 0x01,
    Placement     = 0x02,
    Tiled         = 0x04,
    sRGB          = 0x08,
    LodFromRender = 0x10,
    Cube          = 0x40,
    Watch         = 0x8000,
}
