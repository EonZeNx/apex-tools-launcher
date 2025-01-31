namespace ApexFormat.RTPC.V03.Enum;

/// <summary>
/// Assume 4-byte padding unless stated otherwise
/// </summary>
public enum ERtpcV03Variant : byte
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

public static class ERtpcV03VariantLibrary
{
    public static readonly Dictionary<ERtpcV03Variant, bool> PrimitiveMap = new()
    {
        { ERtpcV03Variant.Unassigned,       true },
        { ERtpcV03Variant.UInteger32,       true },
        { ERtpcV03Variant.Float32,          true },
        { ERtpcV03Variant.String,           false },
        { ERtpcV03Variant.Vector2,          false },
        { ERtpcV03Variant.Vector3,          false },
        { ERtpcV03Variant.Vector4,          false },
        { ERtpcV03Variant.Matrix3X3,        false },
        { ERtpcV03Variant.Matrix4X4,        false },
        { ERtpcV03Variant.UInteger32Array,  false },
        { ERtpcV03Variant.Float32Array,     false },
        { ERtpcV03Variant.ByteArray,        false },
        { ERtpcV03Variant.Deprecated,       false },
        { ERtpcV03Variant.ObjectId,         false },
        { ERtpcV03Variant.Events,           false },
        { ERtpcV03Variant.Total,            true },
    };
    
    public static readonly Dictionary<ERtpcV03Variant, string> XNameMap = new()
    {
        { ERtpcV03Variant.Unassigned,       "unassigned" },
        { ERtpcV03Variant.UInteger32,       "int" },
        { ERtpcV03Variant.Float32,          "float" },
        { ERtpcV03Variant.String,           "string" },
        { ERtpcV03Variant.Vector2,          "vec2" },
        { ERtpcV03Variant.Vector3,          "vec3" },
        { ERtpcV03Variant.Vector4,          "vec4" },
        { ERtpcV03Variant.Matrix3X3,        "mat3" },
        { ERtpcV03Variant.Matrix4X4,        "mat4" },
        { ERtpcV03Variant.UInteger32Array,  "int[]" },
        { ERtpcV03Variant.Float32Array,     "float[]" },
        { ERtpcV03Variant.ByteArray,        "byte[]" },
        { ERtpcV03Variant.Deprecated,       "dep" },
        { ERtpcV03Variant.ObjectId,         "oid" },
        { ERtpcV03Variant.Events,           "events" },
        { ERtpcV03Variant.Total,            "total" },
    };
    
    public static Dictionary<string, ERtpcV03Variant> FromXNameMap = XNameMap.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);

    public static readonly Dictionary<ERtpcV03Variant, int> VariantAlignment = new()
    {
        { ERtpcV03Variant.Unassigned,       4 },
        { ERtpcV03Variant.UInteger32,       0 },
        { ERtpcV03Variant.Float32,          0 },
        { ERtpcV03Variant.String,           0 },
        { ERtpcV03Variant.Vector2,          4 },
        { ERtpcV03Variant.Vector3,          4 },
        { ERtpcV03Variant.Vector4,          4 },
        { ERtpcV03Variant.Matrix3X3,        4 },
        { ERtpcV03Variant.Matrix4X4,        16 },
        { ERtpcV03Variant.UInteger32Array,  4 },
        { ERtpcV03Variant.Float32Array,     4 },
        { ERtpcV03Variant.ByteArray,        16 },
        { ERtpcV03Variant.Deprecated,       0 },
        { ERtpcV03Variant.ObjectId,         4 },
        { ERtpcV03Variant.Events,           4 },
        { ERtpcV03Variant.Total,            0 },
    };
    
    public static ERtpcV03Variant ReadRtpcV03Variant(this Stream stream)
    {
        var result = (ERtpcV03Variant) stream.ReadByte();
        
        return result;
    }
    
    public static bool IsPrimitive(this ERtpcV03Variant variant)
    {
        return PrimitiveMap.GetValueOrDefault(variant, true);
    }
    
    public static string ToXName(this ERtpcV03Variant variant)
    {
        return XNameMap.GetValueOrDefault(variant, "failed");
    }
    
    public static ERtpcV03Variant FromXName(string xmlString)
    {
        return FromXNameMap.GetValueOrDefault(xmlString, ERtpcV03Variant.Unassigned);
    }
    
    public static int Alignment(this ERtpcV03Variant variant)
    {
        return VariantAlignment.GetValueOrDefault(variant, 4);
    }
}