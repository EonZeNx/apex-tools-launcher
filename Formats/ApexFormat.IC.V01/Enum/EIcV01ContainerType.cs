namespace ApexFormat.IC.V01.Enum;

public enum EIcV01ContainerType : ushort
{
    Unk0       = 0x03,
    Collection = 0x04,
    Property   = 0x05,
}

public static class EIcV01ContainerTypeExtensions
{
    public static Dictionary<EIcV01ContainerType, string> ContainerTypeToXmlString = new()
    {
        { EIcV01ContainerType.Unk0,       "unk0" },
        { EIcV01ContainerType.Collection, "collection" },
        { EIcV01ContainerType.Property,   "property" },
    };

    public static string XmlString(this EIcV01ContainerType containerType)
    {
        return ContainerTypeToXmlString.GetValueOrDefault(containerType, "failed");
    }
}