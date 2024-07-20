using ATL.Core.Class;
using CommunityToolkit.HighPerformance;
using RustyOptions;

namespace ApexFormat.TAB.V02;

/// <summary>
/// Structure:
/// <br/>NameHash - <see cref="uint"/>
/// <br/>Offset - <see cref="uint"/>
/// <br/>Size - <see cref="uint"/>
/// </summary>
public class TabV02Entry : ISizeOf
{
    public uint NameHash = 0;
    public uint Offset = 0;
    public uint Size = 0;
    
    public static int SizeOf() => sizeof(uint) + sizeof(uint) + sizeof(uint);
}

public static class TabV02EntryExtensions
{
    public static Option<TabV02Entry> ReadTabV02Entry(this Stream stream)
    {
        if (stream.Length < TabV02Header.SizeOf())
        {
            return Option<TabV02Entry>.None;
        }
        
        if (stream.Length - stream.Position < TabV02Header.SizeOf())
        {
            return Option<TabV02Entry>.None;
        }
        
        var result = new TabV02Entry
        {
            NameHash = stream.Read<uint>(),
            Offset = stream.Read<uint>(),
            Size = stream.Read<uint>(),
        };

        return Option.Some(result);
    }
}