using ATL.Core.Class;
using CommunityToolkit.HighPerformance;
using RustyOptions;

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

public static class AdfV04EnumExtensions
{
    public static Option<AdfV04Enum> ReadAdfV04Enum(this Stream stream)
    {
        if (stream.Length - stream.Position < AdfV04Enum.SizeOf())
        {
            return Option<AdfV04Enum>.None;
        }

        var result = new AdfV04Enum
        {
            NameIndex = stream.Read<ulong>(),
            Value = stream.Read<uint>(),
        };

        return Option.Some(result);
    }
}