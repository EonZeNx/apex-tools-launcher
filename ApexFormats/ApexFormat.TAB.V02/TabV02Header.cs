using ATL.Core.Class;
using CommunityToolkit.HighPerformance;
using RustyOptions;

namespace ApexFormat.TAB.V02;

public static class TabV02HeaderConstants
{
    public static uint Magic = 0x00424154; // "TAB"
    public static ushort MajorVersion = 2;
    public static ushort MinorVersion = 1;
    public static int Alignment = 0x1000;
}

/// <summary>
/// Structure:
/// <br/>Magic - <see cref="uint"/>
/// <br/>MajorVersion - <see cref="ushort"/>
/// <br/>MinorVersion - <see cref="ushort"/>
/// <br/>Alignment - <see cref="int"/>
/// </summary>
public class TabV02Header : ISizeOf
{
    public uint Magic = TabV02HeaderConstants.Magic;
    public ushort MajorVersion = TabV02HeaderConstants.MajorVersion;
    public ushort MinorVersion = TabV02HeaderConstants.MinorVersion;
    public int Alignment = TabV02HeaderConstants.Alignment;

    public static int SizeOf() => sizeof(uint) + sizeof(ushort) + sizeof(ushort) + sizeof(int);
}

public static class TabV02HeaderExtensions
{
    /// <summary>
    /// Validates a TABv02Header
    /// </summary>
    /// <returns>Number, 0 = valid, 0 > invalid, 1+ valid with issue</returns>
    public static Option<TabV02Header> ReadTabV02Header(this Stream stream)
    {
        if (stream.Length < TabV02Header.SizeOf())
        {
            return Option<TabV02Header>.None;
        }
        
        if (stream.Length - stream.Position < TabV02Header.SizeOf())
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
        
        if (result.Magic != TabV02HeaderConstants.Magic)
        {
            return Option<TabV02Header>.None;
        }

        if (result.MajorVersion != TabV02HeaderConstants.MajorVersion)
        {
            return Option<TabV02Header>.None;
        }

        return Option.Some(result);
    }
}