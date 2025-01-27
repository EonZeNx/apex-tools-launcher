using System.Xml.Linq;
using ApexFormat.ADF.V04.Enums;
using ApexToolsLauncher.Core.Class;
using ApexToolsLauncher.Core.Extensions;
using ApexToolsLauncher.Core.Libraries.XBuilder;
using CommunityToolkit.HighPerformance;
using RustyOptions;

namespace ApexFormat.ADF.V04.Class;

/// <summary>
/// <remarks>
///  <list type="table">
///    <listheader>
///      <term>Property</term><description>Type</description>
///    </listheader>
///    <item>
///      <term><c>Type</c></term><description><see cref="EAdfV04Type"/></description>
///    </item>
///    <item>
///      <term><c>Size</c></term><description><see cref="uint"/></description>
///    </item>
///    <item>
///      <term><c>Alignment</c></term><description><see cref="uint"/></description>
///    </item>
///    <item>
///      <term><c>TypeHash</c></term><description><see cref="uint"/></description>
///    </item>
///    <item>
///      <term><c>NameIndex</c></term><description><see cref="ulong"/></description>
///    </item>
///    <item>
///      <term><c>Flags</c></term><description><see cref="ushort"/></description>
///    </item>
///    <item>
///      <term><c>ScalarType</c></term><description><see cref="EAdfV04ScalarType"/></description>
///    </item>
///    <item>
///      <term><c>ScalarTypeHash</c></term><description><see cref="uint"/></description>
///    </item>
///    <item>
///      <term><c>BitCountOrArrayLength</c></term><description><see cref="uint"/></description>
///    </item>
///    <item>
///      <term><c>MemberCountOrDataAlign</c></term><description><see cref="uint"/></description>
///    </item>
///    <item>
///      <term><c>Members</c></term><description><see cref="AdfV04Member"/>[]</description>
///    </item>
///  </list>
/// </remarks>
/// </summary>
public class AdfV04Type
{
    public EAdfV04Type Type = EAdfV04Type.Scalar;
    public uint Size = 0;
    public uint Alignment = 0;
    public uint TypeHash = 0;
    public ulong NameIndex = 0;
    public ushort Flags = 0;
    public EAdfV04ScalarType ScalarType = EAdfV04ScalarType.Signed;
    public uint ScalarTypeHash = 0;
    // bit count for Bitfields, array length for Inline array
    public uint BitCountOrArrayLength = 0;
    // member count only for Struct & Enum
    public uint MemberCountOrDataAlign = 0;
    
    // Members and EnumFlags share the same location
    public AdfV04Member[] Members = [];
    public AdfV04Enum[] EnumFlags = [];

    public string Name { get; set; } = string.Empty;
    public string SafeName => Name.Trim().Trim((char) 0x00);
}

public static class AdfV04TypeLibrary
{
    public const uint SizeOf = sizeof(EAdfV04Type) // Type
                               + sizeof(uint) // Size
                               + sizeof(uint) // Alignment
                               + sizeof(uint) // TypeHash
                               + sizeof(ulong) // NameIndex
                               + sizeof(ushort) // Flags
                               + sizeof(EAdfV04ScalarType) // ScalarType
                               + sizeof(uint) // ScalarTypeHash
                               + sizeof(uint) // BitCountOrArrayLength
                               + sizeof(uint); // MemberCountOrDataAlign
    
    public const string XName = "type";
    
    public static Option<AdfV04Type> ReadAdfV04Type(this Stream stream)
    {
        if (!stream.CouldRead(SizeOf))
        {
            return Option<AdfV04Type>.None;
        }
        
        var result = new AdfV04Type
        {
            Type = stream.Read<EAdfV04Type>(),
            Size = stream.Read<uint>(),
            Alignment = stream.Read<uint>(),
            TypeHash = stream.Read<uint>(),
            NameIndex = stream.Read<ulong>(),
            Flags = stream.Read<ushort>(),
            ScalarType = stream.Read<EAdfV04ScalarType>(),
            ScalarTypeHash = stream.Read<uint>(),
            BitCountOrArrayLength = stream.Read<uint>(),
            MemberCountOrDataAlign = stream.Read<uint>(),
        };

        // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
        switch (result.Type)
        {
        case EAdfV04Type.Struct:
            result.Members = new AdfV04Member[result.MemberCountOrDataAlign];
            for (var i = 0; i < result.MemberCountOrDataAlign; i++)
            {
                var optionMember = stream.ReadAdfV04Member();
                if (!optionMember.IsSome(out var member))
                    continue;

                result.Members[i] = member;
            }
            break;
        case EAdfV04Type.Enum:
            result.EnumFlags = new AdfV04Enum[result.MemberCountOrDataAlign];
            for (var i = 0; i < result.MemberCountOrDataAlign; i += 1)
            {
                var optionEnumeration = stream.ReadAdfV04Enum();
                if (!optionEnumeration.IsSome(out var enumeration))
                    continue;

                result.EnumFlags[i] = enumeration;
            }
            break;
        }

        return Option.Some(result);
    }

    public static XElement ToXElement(this AdfV04Type adfV04Type)
    {
        var xe = XElementBuilder.Create(XName)
            .WithAttribute("type", adfV04Type.Type.ToXName())
            .WithAttribute("size", adfV04Type.Size.ToString())
            .WithAttribute("alignment", adfV04Type.Alignment.ToString())
            .WithAttribute("typeHash", adfV04Type.TypeHash.ToString())
            .WithAttribute("name",
                () => !string.IsNullOrEmpty(adfV04Type.SafeName)
                    ? Option.Some(adfV04Type.SafeName) : Option.None<string>())
            .WithAttribute("nameIndex", adfV04Type.NameIndex.ToString())
            .WithAttribute("flags", adfV04Type.Flags.ToString())
            .WithAttribute("scalarType",
                () => adfV04Type.Type == EAdfV04Type.Scalar
                        ? Option.Some(adfV04Type.ScalarType.ToXName()) : Option.None<string>())
            .WithAttribute("scalarTypeHash",
                () => adfV04Type.Type == EAdfV04Type.Scalar
                    ? Option.Some(adfV04Type.ScalarTypeHash.ToString()) : Option.None<string>())
            .WithAttribute("bitCountOrArrayLength",
                () => adfV04Type.BitCountOrArrayLength != 0
                    ? Option.Some(adfV04Type.BitCountOrArrayLength.ToString()) : Option.None<string>())
            .WithAttribute("memberCountOrDataAlign",
                () => adfV04Type.MemberCountOrDataAlign != 0
                    ? Option.Some(adfV04Type.MemberCountOrDataAlign.ToString()) : Option.None<string>())
            .WithChildren(adfV04Type.Members, member => member.ToXElement())
            .Build();

        return xe;
    }

    public static void FindOrInsertName(this AdfV04Type adfV04Type, ref string[] stringTable, bool recursive = true)
    {
        if ((uint) adfV04Type.NameIndex >= stringTable.Length)
        { // inbuilt types do not have access to string table at creation
            stringTable = stringTable.Append(adfV04Type.Name).ToArray();
            adfV04Type.NameIndex = (ulong) stringTable.Length - 1;
            return;
        }
        
        adfV04Type.Name = stringTable[adfV04Type.NameIndex];

        if (!recursive)
            return;
        
        foreach (var member in adfV04Type.Members)
        {
            member.TryFindName(stringTable);
        }
    }
}