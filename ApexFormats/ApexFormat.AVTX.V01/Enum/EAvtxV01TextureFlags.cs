namespace ApexFormat.AVTX.V01.Enum;

[Flags]
public enum EAvtxV01TextureFlags : uint
{
    Streamed      = 0x1,
    Placement     = 0x2,
    Tiled         = 0x4,
    sRGB          = 0x8,
    LodFromRender = 0x10,
    Cube          = 0x40,
    Watch         = 0x8000,
}
