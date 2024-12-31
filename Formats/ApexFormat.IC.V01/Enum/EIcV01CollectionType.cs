namespace ApexFormat.IC.V01.Enum;

public enum EIcV01CollectionType : ushort
{
    Unk0       = 0x03,
    Container  = 0x04,
    Property   = 0x05,
}

public static class EIcV01CollectionTypeExtensions
{
    public static Dictionary<EIcV01CollectionType, string> TypeToXmlString = new()
    {
        { EIcV01CollectionType.Unk0,       "unk0" },
        { EIcV01CollectionType.Container,  "container" },
        { EIcV01CollectionType.Property,   "property" },
    };

    public static string ToXmlString(this EIcV01CollectionType collectionType)
    {
        return TypeToXmlString.GetValueOrDefault(collectionType, "failed");
    }
}