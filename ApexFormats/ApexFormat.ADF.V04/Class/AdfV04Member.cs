using ATL.Core.Class;

namespace ApexFormat.ADF.V04.Class;

public static class AdfV04MemberConstants
{
    public const uint Offset = 24;
}

/// <summary>
/// Structure:
/// <br/>NameIndex - <see cref="ulong"/>
/// <br/>TypeHash - <see cref="uint"/>
/// <br/>Align - <see cref="uint"/>
/// <br/>Offset - <see cref="uint"/>
/// <br/>BitOffset - <see cref="uint"/>
/// <br/>DefaultValue - <see cref="ulong"/>
/// </summary>
public class AdfV04Member : ISizeOf
{
    public ulong NameIndex         = 0;
    public uint TypeHash      = 0;
    public uint Align         = 0;
    public uint Offset        = AdfV04MemberConstants.Offset;
    public uint BitOffset     = 8;
    public ulong DefaultValue = 0;

    public static uint SizeOf()
    {
        return sizeof(ulong) + // NameIndex
               sizeof(uint) + // TypeHash
               sizeof(uint) + // Align
               sizeof(uint) + // Offset
               sizeof(uint) + // BitOffset
               sizeof(ulong); // DefaultValue
    }
}