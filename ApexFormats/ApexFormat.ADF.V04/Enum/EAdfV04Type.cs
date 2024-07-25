namespace ApexFormat.ADF.V04.Enum;

public enum EAdfV04Type : uint
{
    Scalar       = 0x0,
    Struct       = 0x1,
    Pointer      = 0x2,
    Array        = 0x3,
    InlineArray  = 0x4,
    String       = 0x5,
    Recursive    = 0x6,
    Bitfield     = 0x7,
    Enum         = 0x8,
    StringHash   = 0x9,
    Deferred     = 0xA,
}
