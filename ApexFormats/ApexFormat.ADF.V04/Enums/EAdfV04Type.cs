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

public static class AdfV04TypeExtensions 
{
    public static readonly Dictionary<EAdfV04Type, string> EnumToXString = Enum.GetValues(typeof(EAdfV04Type))
        .Cast<EAdfV04Type>()
        .ToDictionary(cc => cc, cc => cc.ToString().ToLower());

    public static readonly Dictionary<string, EAdfV04Type> XStringToEnum =
        EnumToXString.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);
    
    public static EAdfV04Type ToAdfV04Type(this string str)
    {
        return XStringToEnum.GetValueOrDefault(str, EAdfV04Type.Scalar);
    }
    
    public static string ToXString(this EAdfV04Type variableType)
    {
        return EnumToXString.GetValueOrDefault(variableType, "unknown");
    }
}