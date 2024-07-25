using ATL.Core.Class;

namespace ApexFormat.ADF.V04.Class;

/// <summary>
/// Structure:
/// <br/>NameIndex - <see cref="ulong"/>
/// <br/>Value - <see cref="uint"/>
/// </summary>
public class AdfV04Enum : ISizeOf
{
    public ulong NameIndex = 0;
    public uint Value = 0;

    public static uint SizeOf()
    {
        return sizeof(ulong) + // NameIndex
               sizeof(uint); // Value
    }
}
