namespace ApexFormat.IRTPC.V14.Enum;

public enum EIrtpcV14VariantType : byte
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

public static class IrtpcV14VariantTypeExtensions
{
    public static Dictionary<EIrtpcV14VariantType, string> VariantToXmlString = new()
    {
        { EIrtpcV14VariantType.Unassigned, "unassigned" },
        { EIrtpcV14VariantType.Integer32, "int" },
        { EIrtpcV14VariantType.Float32, "float" },
        { EIrtpcV14VariantType.String, "string" },
        { EIrtpcV14VariantType.Vector2, "vec2" },
        { EIrtpcV14VariantType.Vector3, "vec3" },
        { EIrtpcV14VariantType.Vector4, "vec4" },
        { EIrtpcV14VariantType.Matrix3X4, "mat" },
        { EIrtpcV14VariantType.Events, "events" }
    };

    public static string XmlString(this EIrtpcV14VariantType variantType)
    {
        return VariantToXmlString.GetValueOrDefault(variantType, "failed");
    }
}