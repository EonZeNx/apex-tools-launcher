using ApexFormat.ADF.V04.Enums;
using ApexToolsLauncher.Core.Class;
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
public class AdfV04Type : ISizeOf
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

    public string Name { get; set; } = "EMPTY";

    public static uint SizeOf()
    {
        return sizeof(EAdfV04Type) + // Magic
               sizeof(uint) + // Size
               sizeof(uint) + // TypeHash
               sizeof(ulong) + // NameIndex
               sizeof(ushort) + // Flags
               sizeof(EAdfV04ScalarType) + // ScalarType
               sizeof(uint) + // ScalarTypeHash
               sizeof(uint) + // BitCountOrArrayLength
               sizeof(uint); // MemberCountOrDataAlign
    }
}

public static class AdfV04TypeExtensions
{
    public static Option<AdfV04Type> ReadAdfV04Type(this Stream stream)
    {
        if (stream.Length - stream.Position < AdfV04HeaderLibrary.SizeOf)
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
}