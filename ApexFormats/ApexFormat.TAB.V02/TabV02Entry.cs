using CommunityToolkit.HighPerformance;

namespace ApexFormat.TAB.V02;

/// <summary>
/// Structure:
/// <br/>NameHash - <see cref="uint"/>
/// <br/>Offset - <see cref="uint"/>
/// <br/>Size - <see cref="uint"/>
/// </summary>
public class TabV02Entry
{
    public uint NameHash = 0;
    public uint Offset = 0;
    public uint Size = 0;
    
    public static uint SizeOf() => sizeof(uint) + sizeof(uint) + sizeof(uint);
}

public static class TabV02EntryExtensions
{
    public static TabV02Entry ReadTabV02Entry(this Stream stream)
    {
        var result = new TabV02Entry
        {
            NameHash = stream.Read<uint>(),
            Offset = stream.Read<uint>(),
            Size = stream.Read<uint>(),
        };

        return result;
    }
}