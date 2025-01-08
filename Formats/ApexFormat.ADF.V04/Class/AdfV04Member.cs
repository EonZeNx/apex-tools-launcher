using ATL.Core.Class;
using CommunityToolkit.HighPerformance;
using RustyOptions;

namespace ApexFormat.ADF.V04.Class;

/// <summary>
/// Structure:
/// <br/>NameIndex - <see cref="ulong"/>
/// <br/>TypeHash - <see cref="uint"/>
/// <br/>Alignment - <see cref="uint"/>
/// <br/>OffsetAndBitOffset - <see cref="uint"/>
/// <br/>Note: Offset uses the first 24 bits, bit offset uses the last 8
/// <br/>Flags - <see cref="uint"/>
/// <br/>DefaultValue - <see cref="ulong"/>
/// </summary>
public class AdfV04Member : ISizeOf
{
    public ulong NameIndex         = 0;
    public uint TypeHash           = 0;
    public uint Alignment          = 0;
    public uint OffsetAndBitOffset = 0;
    public uint Flags              = 0;
    public ulong DefaultValue      = 0;

    public string Name { get; set; } = "EMPTY";

    public uint Offset {
        get => OffsetAndBitOffset & 0xFFFFFF; // Get the lower 24 bits
        set => OffsetAndBitOffset = (OffsetAndBitOffset & 0xFF000000) | (value & 0xFFFFFF); // Set the lower 24 bits
    }

    public uint BitOffset {
        get => (OffsetAndBitOffset >> 24) & 0xFF; // Get the upper 8 bits
        set => OffsetAndBitOffset = (OffsetAndBitOffset & 0xFFFFFF) | ((value & 0xFF) << 24); // Set the upper 8 bits
    }

    public override string ToString()
    {
        return $"'{Name}' i: {NameIndex} type: {TypeHash}";
    }

    public static uint SizeOf()
    {
        return sizeof(ulong) + // NameIndex
               sizeof(uint) + // TypeHash
               sizeof(uint) + // Align
               sizeof(uint) + // OffsetAndBitOffset
               sizeof(uint) + // Flags
               sizeof(ulong); // DefaultValue
    }
}

public static class AdfV04MemberExtensions
{
    public static Option<AdfV04Member> ReadAdfV04Member(this Stream stream)
    {
        if (stream.Length - stream.Position < AdfV04Header.SizeOf())
        {
            return Option<AdfV04Member>.None;
        }
        
        var result = new AdfV04Member
        {
            NameIndex = stream.Read<ulong>(),
            TypeHash = stream.Read<uint>(),
            Alignment = stream.Read<uint>(),
            OffsetAndBitOffset = stream.Read<uint>(),
            Flags = stream.Read<uint>(),
            DefaultValue = stream.Read<ulong>()
        };

        return Option.Some(result);
    }
}