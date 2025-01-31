namespace ApexFormat.ADF.V04.Enums;

public enum EAdfV04ScalarType : ushort
{
    Signed   = 0x0,
    Unsigned = 0x1,
    Float    = 0x2,
}

public static class EAdfV04ScalarTypeLibrary 
{
    
    public static readonly Dictionary<EAdfV04ScalarType, string> XNameMap = new()
    {
        { EAdfV04ScalarType.Signed,   "signed" },
        { EAdfV04ScalarType.Unsigned, "unsigned" },
        { EAdfV04ScalarType.Float,    "float" },
    };
    
    public static Dictionary<string, EAdfV04ScalarType> FromXNameMap = XNameMap.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);
    
    public static string ToXName(this EAdfV04ScalarType scalarType)
    {
        return XNameMap.GetValueOrDefault(scalarType, "failed");
    }
    
    public static EAdfV04ScalarType FromXName(string xmlString)
    {
        return FromXNameMap.GetValueOrDefault(xmlString, EAdfV04ScalarType.Signed);
    }
}