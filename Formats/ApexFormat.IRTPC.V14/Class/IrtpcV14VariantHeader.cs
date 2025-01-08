using ApexFormat.IRTPC.V14.Enum;
using ApexToolsLauncher.Core.Class;
using CommunityToolkit.HighPerformance;
using RustyOptions;

namespace ApexFormat.IRTPC.V14.Class;

/// <summary>
/// Structure:
/// <br/>NameHash - <see cref="uint"/>
/// <br/>VariantType - <see cref="EIrtpcV14VariantType"/>
/// </summary>
public class IrtpcV14VariantHeader : ISizeOf
{
    public uint NameHash = 0;
    public EIrtpcV14VariantType VariantType = EIrtpcV14VariantType.Unassigned;

    public static uint SizeOf()
    {
        return sizeof(uint) + // NameHash
               sizeof(EIrtpcV14VariantType); // VariantType
    }
}

public static class IrtpcV14VariantHeaderExtensions
{
    public static Option<IrtpcV14VariantHeader> ReadIrtpcV14VariantHeader(this Stream stream)
    {
        if (stream.Length - stream.Position < IrtpcV14VariantHeader.SizeOf())
        {
            return Option<IrtpcV14VariantHeader>.None;
        }
        
        var result = new IrtpcV14VariantHeader
        {
            NameHash = stream.Read<uint>(),
            VariantType = stream.Read<EIrtpcV14VariantType>(),
        };

        return Option.Some(result);
    }
}