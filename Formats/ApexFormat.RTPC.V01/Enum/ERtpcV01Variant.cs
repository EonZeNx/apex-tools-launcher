namespace ApexFormat.RTPC.V01.Enum;

/// <summary>
/// Assume 4-byte padding unless stated otherwise
/// </summary>
public enum ERtpcV01Variant : byte
{
    Unassigned =        0x00, // No pad
    UInteger32 =        0x01, // No pad
    Float32 =           0x02, // No pad
    String =            0x03, // No pad
    Vector2 =           0x04,
    Vector3 =           0x05,
    Vector4 =           0x06,
    Matrix3X3 =         0x07,
    Matrix4X4 =         0x08, // 16-byte pad
    UInteger32Array =   0x09,
    Float32Array =      0x0A,
    ByteArray =         0x0B, // 16-byte pad
    Deprecated =        0x0C,
    ObjectId =          0x0D,
    Events =            0x0E,
    Total =             0x0F
}

public static class RtpcV01VariantLibrary
{
    public static readonly Dictionary<ERtpcV01Variant, bool> PrimitiveMap = new()
    {
        { ERtpcV01Variant.Unassigned,       true },
        { ERtpcV01Variant.UInteger32,       true },
        { ERtpcV01Variant.Float32,          true },
        { ERtpcV01Variant.String,           false },
        { ERtpcV01Variant.Vector2,          false },
        { ERtpcV01Variant.Vector3,          false },
        { ERtpcV01Variant.Vector4,          false },
        { ERtpcV01Variant.Matrix3X3,        false },
        { ERtpcV01Variant.Matrix4X4,        false },
        { ERtpcV01Variant.UInteger32Array,  false },
        { ERtpcV01Variant.Float32Array,     false },
        { ERtpcV01Variant.ByteArray,        false },
        { ERtpcV01Variant.Deprecated,       false },
        { ERtpcV01Variant.ObjectId,         false },
        { ERtpcV01Variant.Events,           false },
        { ERtpcV01Variant.Total,            true },
    };
    
    public static readonly Dictionary<ERtpcV01Variant, string> XNameMap = new()
    {
        { ERtpcV01Variant.Unassigned,       "unassigned" },
        { ERtpcV01Variant.UInteger32,       "int" },
        { ERtpcV01Variant.Float32,          "float" },
        { ERtpcV01Variant.String,           "string" },
        { ERtpcV01Variant.Vector2,          "vec2" },
        { ERtpcV01Variant.Vector3,          "vec3" },
        { ERtpcV01Variant.Vector4,          "vec4" },
        { ERtpcV01Variant.Matrix3X3,        "mat3" },
        { ERtpcV01Variant.Matrix4X4,        "mat4" },
        { ERtpcV01Variant.UInteger32Array,  "int[]" },
        { ERtpcV01Variant.Float32Array,     "float[]" },
        { ERtpcV01Variant.ByteArray,        "byte[]" },
        { ERtpcV01Variant.Deprecated,       "dep" },
        { ERtpcV01Variant.ObjectId,         "oid" },
        { ERtpcV01Variant.Events,           "events" },
        { ERtpcV01Variant.Total,            "total" },
    };

    public static Dictionary<string, ERtpcV01Variant> FromXNameMap = XNameMap.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);
    
    public static readonly Dictionary<ERtpcV01Variant, int> VariantAlignment = new()
    {
        { ERtpcV01Variant.Unassigned,       4 },
        { ERtpcV01Variant.UInteger32,       0 },
        { ERtpcV01Variant.Float32,          0 },
        { ERtpcV01Variant.String,           0 },
        { ERtpcV01Variant.Vector2,          4 },
        { ERtpcV01Variant.Vector3,          4 },
        { ERtpcV01Variant.Vector4,          4 },
        { ERtpcV01Variant.Matrix3X3,        4 },
        { ERtpcV01Variant.Matrix4X4,        16 },
        { ERtpcV01Variant.UInteger32Array,  4 },
        { ERtpcV01Variant.Float32Array,     4 },
        { ERtpcV01Variant.ByteArray,        16 },
        { ERtpcV01Variant.Deprecated,       0 },
        { ERtpcV01Variant.ObjectId,         4 },
        { ERtpcV01Variant.Events,           4 },
        { ERtpcV01Variant.Total,            0 },
    };
    
    public static ERtpcV01Variant ReadRtpcV01Variant(this Stream stream)
    {
        var result = (ERtpcV01Variant) stream.ReadByte();
        
        return result;
    }
    
    public static bool IsPrimitive(this ERtpcV01Variant variant)
    {
        return PrimitiveMap.GetValueOrDefault(variant, true);
    }
    
    public static string ToXName(this ERtpcV01Variant variant)
    {
        return XNameMap.GetValueOrDefault(variant, "failed");
    }
    
    public static ERtpcV01Variant FromXName(string xmlString)
    {
        return FromXNameMap.GetValueOrDefault(xmlString, ERtpcV01Variant.Unassigned);
    }
    
    public static int Alignment(this ERtpcV01Variant variant)
    {
        return VariantAlignment.GetValueOrDefault(variant, 4);
    }
}