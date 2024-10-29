namespace ApexFormat.ADF.V04.Enums;

[Flags]
public enum EAdfV04TypeFlags : uint
{
    Default        = 0x0,
    SimplePodRead  = 0x1,
    SimplePodWrite = 0x2,
    NoStringHashes = 0x4,
    IsFinalised    = 0x8,
}
