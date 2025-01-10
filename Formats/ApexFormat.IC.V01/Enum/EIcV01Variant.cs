namespace ApexFormat.IC.V01.Enum;

public enum EIcV01Variant : byte
{
    Unassigned =        0x00,
    UInteger32 =        0x01,
    Float32 =           0x02,
    String =            0x03,
    Vector2 =           0x04,
    Vector3 =           0x05,
    Vector4 =           0x06,
    Matrix3X3 =         0x07,
    Matrix3X4 =         0x08,
    UInteger32Array =   0x09,
    Float32Array =      0x0A,
    ByteArray =         0x0B,
    Deprecated =        0x0C,
    ObjectId =          0x0D,
    Events =            0x0E,
    Total =             0x0F
}

public static class IcV01VariantLibrary
{
    public static readonly Dictionary<EIcV01Variant, bool> PrimitiveMap = new()
    {
        { EIcV01Variant.Unassigned,       true },
        { EIcV01Variant.UInteger32,       true },
        { EIcV01Variant.Float32,          true },
        { EIcV01Variant.String,           false },
        { EIcV01Variant.Vector2,          false },
        { EIcV01Variant.Vector3,          false },
        { EIcV01Variant.Vector4,          false },
        { EIcV01Variant.Matrix3X3,        false },
        { EIcV01Variant.Matrix3X4,        false },
        { EIcV01Variant.UInteger32Array,  false },
        { EIcV01Variant.Float32Array,     false },
        { EIcV01Variant.ByteArray,        false },
        { EIcV01Variant.Deprecated,       false },
        { EIcV01Variant.ObjectId,         false },
        { EIcV01Variant.Events,           false },
        { EIcV01Variant.Total,            true },
    };
    
    public static Dictionary<EIcV01Variant, string> XNameMap = new()
    {
        { EIcV01Variant.Unassigned,       "unassigned" },
        { EIcV01Variant.UInteger32,       "int" },
        { EIcV01Variant.Float32,          "float" },
        { EIcV01Variant.String,           "string" },
        { EIcV01Variant.Vector2,          "vec2" },
        { EIcV01Variant.Vector3,          "vec3" },
        { EIcV01Variant.Vector4,          "vec4" },
        { EIcV01Variant.Matrix3X3,        "mat3x3" },
        { EIcV01Variant.Matrix3X4,        "mat3x4" },
        { EIcV01Variant.UInteger32Array,  "int[]" },
        { EIcV01Variant.Float32Array,     "float[]" },
        { EIcV01Variant.ByteArray,        "byte[]" },
        { EIcV01Variant.Deprecated,       "dep" },
        { EIcV01Variant.ObjectId,         "oid" },
        { EIcV01Variant.Events,           "events" },
        { EIcV01Variant.Total,            "total" },
    };

    public static Dictionary<string, EIcV01Variant> FromXNameMap = XNameMap.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);

    public static string ToXName(this EIcV01Variant variant)
    {
        return XNameMap.GetValueOrDefault(variant, "failed");
    }
    
    public static EIcV01Variant FromXName(string xmlString)
    {
        return FromXNameMap.GetValueOrDefault(xmlString, EIcV01Variant.Unassigned);
    }
    
    public static bool IsPrimitive(this EIcV01Variant variantType)
    {
        return PrimitiveMap.GetValueOrDefault(variantType, true);
    }
}