namespace ApexFormat.RTPC.V01.Enum;

/// <summary>
/// Assume 4-byte padding unless stated otherwise
/// </summary>
public enum ERtpcV01VariantType : byte
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

public static class RtpcV01VariantTypeExtensions
{
    public static readonly Dictionary<ERtpcV01VariantType, bool> VariantPrimitive = new()
    {
        { ERtpcV01VariantType.Unassigned,       true },
        { ERtpcV01VariantType.UInteger32,       true },
        { ERtpcV01VariantType.Float32,          true },
        { ERtpcV01VariantType.String,           false },
        { ERtpcV01VariantType.Vector2,          false },
        { ERtpcV01VariantType.Vector3,          false },
        { ERtpcV01VariantType.Vector4,          false },
        { ERtpcV01VariantType.Matrix3X3,        false },
        { ERtpcV01VariantType.Matrix4X4,        false },
        { ERtpcV01VariantType.UInteger32Array,  false },
        { ERtpcV01VariantType.Float32Array,     false },
        { ERtpcV01VariantType.ByteArray,        false },
        { ERtpcV01VariantType.Deprecated,       false },
        { ERtpcV01VariantType.ObjectId,         false },
        { ERtpcV01VariantType.Events,           false },
        { ERtpcV01VariantType.Total,            true },
    };
    
    public static readonly Dictionary<ERtpcV01VariantType, string> VariantXmlString = new()
    {
        { ERtpcV01VariantType.Unassigned,       "unassigned" },
        { ERtpcV01VariantType.UInteger32,       "int" },
        { ERtpcV01VariantType.Float32,          "float" },
        { ERtpcV01VariantType.String,           "string" },
        { ERtpcV01VariantType.Vector2,          "vec2" },
        { ERtpcV01VariantType.Vector3,          "vec3" },
        { ERtpcV01VariantType.Vector4,          "vec4" },
        { ERtpcV01VariantType.Matrix3X3,        "mat3" },
        { ERtpcV01VariantType.Matrix4X4,        "mat4" },
        { ERtpcV01VariantType.UInteger32Array,  "int[]" },
        { ERtpcV01VariantType.Float32Array,     "float[]" },
        { ERtpcV01VariantType.ByteArray,        "byte[]" },
        { ERtpcV01VariantType.Deprecated,       "dep" },
        { ERtpcV01VariantType.ObjectId,         "oid" },
        { ERtpcV01VariantType.Events,           "events" },
        { ERtpcV01VariantType.Total,            "total" },
    };
    
    public static readonly Dictionary<ERtpcV01VariantType, int> VariantAlignment = new()
    {
        { ERtpcV01VariantType.Unassigned,       4 },
        { ERtpcV01VariantType.UInteger32,       0 },
        { ERtpcV01VariantType.Float32,          0 },
        { ERtpcV01VariantType.String,           0 },
        { ERtpcV01VariantType.Vector2,          4 },
        { ERtpcV01VariantType.Vector3,          4 },
        { ERtpcV01VariantType.Vector4,          4 },
        { ERtpcV01VariantType.Matrix3X3,        4 },
        { ERtpcV01VariantType.Matrix4X4,        16 },
        { ERtpcV01VariantType.UInteger32Array,  4 },
        { ERtpcV01VariantType.Float32Array,     4 },
        { ERtpcV01VariantType.ByteArray,        16 },
        { ERtpcV01VariantType.Deprecated,       0 },
        { ERtpcV01VariantType.ObjectId,         4 },
        { ERtpcV01VariantType.Events,           4 },
        { ERtpcV01VariantType.Total,            0 },
    };
    
    public static ERtpcV01VariantType ReadRtpcV01VariantType(this Stream stream)
    {
        var result = (ERtpcV01VariantType) stream.ReadByte();
        
        return result;
    }
    
    public static bool IsPrimitive(this ERtpcV01VariantType variantType)
    {
        return VariantPrimitive.GetValueOrDefault(variantType, true);
    }
    
    public static string XmlString(this ERtpcV01VariantType variantType)
    {
        return VariantXmlString.GetValueOrDefault(variantType, "failed");
    }
    
    public static int Alignment(this ERtpcV01VariantType variantType)
    {
        return VariantAlignment.GetValueOrDefault(variantType, 4);
    }
}