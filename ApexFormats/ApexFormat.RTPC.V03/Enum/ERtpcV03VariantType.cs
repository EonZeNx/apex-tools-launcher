namespace ApexFormat.RTPC.V03.Enum;

/// <summary>
/// Assume 4-byte padding unless stated otherwise
/// </summary>
public enum ERtpcV03VariantType : byte
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

public static class ERtpcV03VariantTypeExtensions
{
    public static readonly Dictionary<ERtpcV03VariantType, bool> VariantPrimitive = new()
    {
        { ERtpcV03VariantType.Unassigned,       true },
        { ERtpcV03VariantType.UInteger32,       true },
        { ERtpcV03VariantType.Float32,          true },
        { ERtpcV03VariantType.String,           false },
        { ERtpcV03VariantType.Vector2,          false },
        { ERtpcV03VariantType.Vector3,          false },
        { ERtpcV03VariantType.Vector4,          false },
        { ERtpcV03VariantType.Matrix3X3,        false },
        { ERtpcV03VariantType.Matrix4X4,        false },
        { ERtpcV03VariantType.UInteger32Array,  false },
        { ERtpcV03VariantType.Float32Array,     false },
        { ERtpcV03VariantType.ByteArray,        false },
        { ERtpcV03VariantType.Deprecated,       false },
        { ERtpcV03VariantType.ObjectId,         false },
        { ERtpcV03VariantType.Events,           false },
        { ERtpcV03VariantType.Total,            true },
    };
    
    public static readonly Dictionary<ERtpcV03VariantType, string> VariantXmlString = new()
    {
        { ERtpcV03VariantType.Unassigned,       "unassigned" },
        { ERtpcV03VariantType.UInteger32,       "int" },
        { ERtpcV03VariantType.Float32,          "float" },
        { ERtpcV03VariantType.String,           "string" },
        { ERtpcV03VariantType.Vector2,          "vec2" },
        { ERtpcV03VariantType.Vector3,          "vec3" },
        { ERtpcV03VariantType.Vector4,          "vec4" },
        { ERtpcV03VariantType.Matrix3X3,        "mat3" },
        { ERtpcV03VariantType.Matrix4X4,        "mat4" },
        { ERtpcV03VariantType.UInteger32Array,  "int[]" },
        { ERtpcV03VariantType.Float32Array,     "float[]" },
        { ERtpcV03VariantType.ByteArray,        "byte[]" },
        { ERtpcV03VariantType.Deprecated,       "dep" },
        { ERtpcV03VariantType.ObjectId,         "oid" },
        { ERtpcV03VariantType.Events,           "events" },
        { ERtpcV03VariantType.Total,            "total" },
    };
    
    public static readonly Dictionary<ERtpcV03VariantType, int> VariantAlignment = new()
    {
        { ERtpcV03VariantType.Unassigned,       4 },
        { ERtpcV03VariantType.UInteger32,       0 },
        { ERtpcV03VariantType.Float32,          0 },
        { ERtpcV03VariantType.String,           0 },
        { ERtpcV03VariantType.Vector2,          4 },
        { ERtpcV03VariantType.Vector3,          4 },
        { ERtpcV03VariantType.Vector4,          4 },
        { ERtpcV03VariantType.Matrix3X3,        4 },
        { ERtpcV03VariantType.Matrix4X4,        16 },
        { ERtpcV03VariantType.UInteger32Array,  4 },
        { ERtpcV03VariantType.Float32Array,     4 },
        { ERtpcV03VariantType.ByteArray,        16 },
        { ERtpcV03VariantType.Deprecated,       0 },
        { ERtpcV03VariantType.ObjectId,         4 },
        { ERtpcV03VariantType.Events,           4 },
        { ERtpcV03VariantType.Total,            0 },
    };
    
    public static ERtpcV03VariantType ReadRtpcV03VariantType(this Stream stream)
    {
        var result = (ERtpcV03VariantType) stream.ReadByte();
        
        return result;
    }
    
    public static bool IsPrimitive(this ERtpcV03VariantType variantType)
    {
        return VariantPrimitive.GetValueOrDefault(variantType, true);
    }
    
    public static string XmlString(this ERtpcV03VariantType variantType)
    {
        return VariantXmlString.GetValueOrDefault(variantType, "failed");
    }
    
    public static int Alignment(this ERtpcV03VariantType variantType)
    {
        return VariantAlignment.GetValueOrDefault(variantType, 4);
    }
}