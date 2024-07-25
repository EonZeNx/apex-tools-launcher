using ApexFormat.ADF.V04.Enum;
using ATL.Core.Class;
using CommunityToolkit.HighPerformance;
using RustyOptions;

namespace ApexFormat.ADF.V04.Class;

/// <summary>
/// Structure:
/// <br/>Type - <see cref="EAdfV04Type"/>
/// <br/>Size - <see cref="uint"/>
/// <br/>Alignment - <see cref="uint"/>
/// <br/>TypeHash - <see cref="uint"/>
/// <br/>NameIndex - <see cref="ulong"/>
/// <br/>Flags - <see cref="ushort"/>
/// <br/>ScalarType - <see cref="EAdfV04ScalarType"/>
/// <br/>SubTypeHash - <see cref="uint"/>
/// <br/>BitCountOrArrayLength - <see cref="uint"/>
/// <br/>MemberCountOrDataAlign - <see cref="uint"/>
/// <br/>Members - <see cref="AdfV04Member"/>[]
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
    public uint SubTypeHash = 0;
    // bit count for Bitfields, array length for Inline array
    public uint BitCountOrArrayLength = 0;
    // member count only for Struct & Enum
    public uint MemberCountOrDataAlign = 0;
    public AdfV04Member[] Members = [];

    public static uint SizeOf()
    {
        return sizeof(EAdfV04Type) + // Magic
               sizeof(uint) + // Size
               sizeof(uint) + // TypeHash
               sizeof(ulong) + // NameIndex
               sizeof(ushort) + // Flags
               sizeof(EAdfV04ScalarType) + // ScalarType
               sizeof(uint) + // SubTypeHash
               sizeof(uint) + // BitCountOrArrayLength
               sizeof(uint); // MemberCountOrDataAlign
    }
}

public static class AdfV04TypeExtensions
{
    public static Option<AdfV04Type> ReadAdfV04Type(this Stream stream)
    {
        var result = new AdfV04Type
        {
            Type = stream.Read<EAdfV04Type>(),
            Size = stream.Read<uint>(),
            Alignment = stream.Read<uint>(),
            TypeHash = stream.Read<uint>(),
            NameIndex = stream.Read<ulong>(),
            Flags = stream.Read<ushort>(),
            ScalarType = stream.Read<EAdfV04ScalarType>(),
            SubTypeHash = stream.Read<uint>(),
            BitCountOrArrayLength = stream.Read<uint>(),
            MemberCountOrDataAlign = stream.Read<uint>(),
        };

        return Option.Some(result);
    }

    public static uint DataSize(this AdfV04Type adfType)
    {
        uint memberCount = 0;
        var memberSize = AdfV04Member.SizeOf();

        if (adfType.Type == EAdfV04Type.Struct || adfType.Type == EAdfV04Type.Enum)
        {
            memberCount = adfType.MemberCountOrDataAlign;

            if (adfType.Type == EAdfV04Type.Enum)
            {
                memberSize = AdfV04Enum.SizeOf();
            }
        }

        return AdfV04Type.SizeOf() + (memberCount * memberSize);
    }
}