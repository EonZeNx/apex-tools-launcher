using ApexToolsLauncher.Core.Extensions;
using CommunityToolkit.HighPerformance;
using RustyOptions;

namespace ApexFormat.TAB.V02.Class;

/// <summary>
/// Structure:
/// <br/>Magic - <see cref="uint"/>
/// <br/>MajorVersion - <see cref="ushort"/>
/// <br/>MinorVersion - <see cref="ushort"/>
/// <br/>Alignment - <see cref="int"/>
/// </summary>
public class TabV02Header
{
    public uint Magic = TabV02HeaderLibrary.Magic;
    public ushort MajorVersion = TabV02HeaderLibrary.MajorVersion;
    public ushort MinorVersion = TabV02HeaderLibrary.MinorVersion;
    public int Alignment = TabV02HeaderLibrary.Alignment;
}

public static class TabV02HeaderLibrary
{
    public const int SizeOf = sizeof(uint)
                              + sizeof(ushort)
                              + sizeof(ushort)
                              + sizeof(int);
    
    public const uint Magic = 0x00424154; // "TAB"
    public const ushort MajorVersion = 2;
    public const ushort MinorVersion = 1;
    public const int Alignment = 0x1000;
    
    /// <summary>
    /// Validates a TABv02Header
    /// </summary>
    /// <returns>Number, 0 = valid, 0 > invalid, 1+ valid with issue</returns>
    public static Option<TabV02Header> ReadTabV02Header(this Stream stream)
    {
        if (!stream.CouldRead(SizeOf))
        {
            return Option<TabV02Header>.None;
        }
        
        var result = new TabV02Header
        {
            Magic = stream.Read<uint>(),
            MajorVersion = stream.Read<ushort>(),
            MinorVersion = stream.Read<ushort>(),
            Alignment = stream.Read<int>(),
        };
        
        if (result.Magic != Magic)
        {
            return Option<TabV02Header>.None;
        }

        if (result.MajorVersion != MajorVersion)
        {
            return Option<TabV02Header>.None;
        }

        return Option.Some(result);
    }
}