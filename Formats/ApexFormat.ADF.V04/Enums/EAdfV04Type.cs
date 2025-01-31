namespace ApexFormat.ADF.V04.Enums;

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

public static class AdfV04TypeLibrary 
{
    
    public static readonly Dictionary<EAdfV04Type, string> XNameMap = new()
    {
        { EAdfV04Type.Scalar,      "scalar" },
        { EAdfV04Type.Struct,      "struct" },
        { EAdfV04Type.Pointer,     "pointer" },
        { EAdfV04Type.Array,       "array" },
        { EAdfV04Type.InlineArray, "iarray" },
        { EAdfV04Type.String,      "string" },
        { EAdfV04Type.Recursive,   "recursive" },
        { EAdfV04Type.Bitfield,    "bitfield" },
        { EAdfV04Type.Enum,        "enum" },
        { EAdfV04Type.StringHash,  "stringHash" },
        { EAdfV04Type.Deferred,    "deferred" },
    };
    
    public static Dictionary<string, EAdfV04Type> FromXNameMap = XNameMap.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);
    
    public static string ToXName(this EAdfV04Type adfType)
    {
        return XNameMap.GetValueOrDefault(adfType, "failed");
    }
    
    public static EAdfV04Type FromXName(string xmlString)
    {
        return FromXNameMap.GetValueOrDefault(xmlString, EAdfV04Type.Scalar);
    }
}