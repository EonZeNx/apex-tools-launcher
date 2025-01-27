using System.Xml.Linq;
using ApexToolsLauncher.Core.Class;
using ApexToolsLauncher.Core.Extensions;
using ApexToolsLauncher.Core.Libraries.XBuilder;
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
public class AdfV04Member
{
    public ulong NameIndex         = 0;
    public uint TypeHash           = 0;
    public uint Alignment          = 0;
    public uint OffsetAndBitOffset = 0;
    public uint Flags              = 0;
    public ulong DefaultValue      = 0;

    public string Name { get; set; } = string.Empty;
    public string SafeName => Name.Trim().Trim((char) 0x00);

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
}

public static class AdfV04MemberLibrary
{
    public const uint SizeOf = sizeof(ulong) // NameIndex
                               + sizeof(uint) // TypeHash
                               + sizeof(uint) // Align
                               + sizeof(uint) // OffsetAndBitOffset
                               + sizeof(uint) // Flags
                               + sizeof(ulong); // DefaultValue
    
    public const string XName = "member";
    
    public static Option<AdfV04Member> ReadAdfV04Member(this Stream stream)
    {
        if (!stream.CouldRead(SizeOf))
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

    public static XElement ToXElement(this AdfV04Member member)
    {
        var xe = XElementBuilder.Create(XName)
            .WithAttribute("nameIndex", member.NameIndex.ToString())
            .WithAttribute("name",
                () => !string.IsNullOrEmpty(member.SafeName)
                    ? Option.Some(member.SafeName) : Option.None<string>())
            .WithAttribute("typeHash", member.TypeHash.ToString())
            .WithAttribute("alignment", member.Alignment.ToString())
            .WithAttribute("offsetAndBitOffset", member.OffsetAndBitOffset.ToString())
            .WithAttribute("flags", member.Flags.ToString())
            .WithAttribute("defaultValue", member.DefaultValue.ToString())
            .Build();
        
        return xe;
    }

    public static void TryFindName(this AdfV04Member member, string[] stringTable)
    {
        if ((uint) member.NameIndex >= stringTable.Length)
            return;
        
        member.Name = stringTable[member.NameIndex];
    }
}