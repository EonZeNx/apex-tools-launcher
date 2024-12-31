namespace ApexFormat.IC.V01.Enum;

public enum EIcVariantV01 : byte
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

public static class EIcV01VariantExtensions
{
    public static Dictionary<EIcVariantV01, string> VariantToXmlString = new()
    {
        { EIcVariantV01.Unassigned,       "unassigned" },
        { EIcVariantV01.UInteger32,       "int" },
        { EIcVariantV01.Float32,          "float" },
        { EIcVariantV01.String,           "string" },
        { EIcVariantV01.Vector2,          "vec2" },
        { EIcVariantV01.Vector3,          "vec3" },
        { EIcVariantV01.Vector4,          "vec4" },
        { EIcVariantV01.Matrix3X3,        "mat3x3" },
        { EIcVariantV01.Matrix3X4,        "mat3x4" },
        { EIcVariantV01.UInteger32Array,  "int[]" },
        { EIcVariantV01.Float32Array,     "float[]" },
        { EIcVariantV01.ByteArray,        "byte[]" },
        { EIcVariantV01.Deprecated,       "dep" },
        { EIcVariantV01.ObjectId,         "oid" },
        { EIcVariantV01.Events,           "events" },
        { EIcVariantV01.Total,            "total" },
    };
    
    
    public static readonly Dictionary<EIcVariantV01, bool> VariantPrimitive = new()
    {
        { EIcVariantV01.Unassigned,       true },
        { EIcVariantV01.UInteger32,       true },
        { EIcVariantV01.Float32,          true },
        { EIcVariantV01.String,           false },
        { EIcVariantV01.Vector2,          false },
        { EIcVariantV01.Vector3,          false },
        { EIcVariantV01.Vector4,          false },
        { EIcVariantV01.Matrix3X3,        false },
        { EIcVariantV01.Matrix3X4,        false },
        { EIcVariantV01.UInteger32Array,  false },
        { EIcVariantV01.Float32Array,     false },
        { EIcVariantV01.ByteArray,        false },
        { EIcVariantV01.Deprecated,       false },
        { EIcVariantV01.ObjectId,         false },
        { EIcVariantV01.Events,           false },
        { EIcVariantV01.Total,            true },
    };

    public static string XmlString(this EIcVariantV01 variantV01)
    {
        return VariantToXmlString.GetValueOrDefault(variantV01, "failed");
    }
    
    public static bool IsPrimitive(this EIcVariantV01 variantType)
    {
        return VariantPrimitive.GetValueOrDefault(variantType, true);
    }
}