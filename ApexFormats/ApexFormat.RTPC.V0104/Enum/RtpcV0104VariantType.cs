namespace ApexFormat.RTPC.V0104.Enum;

public enum ERtpcV0104VariantType : byte
{
    Unassigned    = 0x0,
    Integer32     = 0x1,
    Float32       = 0x2,
    String        = 0x3,
    Vector2       = 0x4,
    Vector3       = 0x5,
    Vector4       = 0x6,
    DoNotUse01    = 0x7,
    Matrix3X4     = 0x8,
    Events        = 0xE
}

public static class RtpcV0104VariantTypeExtensions
{
    public static Dictionary<ERtpcV0104VariantType, string> VariantToXmlString = new()
    {
        { ERtpcV0104VariantType.Unassigned, "unassigned" },
        { ERtpcV0104VariantType.Integer32, "int" },
        { ERtpcV0104VariantType.Float32, "float" },
        { ERtpcV0104VariantType.String, "string" },
        { ERtpcV0104VariantType.Vector2, "vec2" },
        { ERtpcV0104VariantType.Vector3, "vec3" },
        { ERtpcV0104VariantType.Vector4, "vec4" },
        { ERtpcV0104VariantType.Matrix3X4, "mat" },
        { ERtpcV0104VariantType.Events, "events" }
    };

    public static string XmlString(this ERtpcV0104VariantType variantType)
    {
        return VariantToXmlString.GetValueOrDefault(variantType, "failed");
    }
}