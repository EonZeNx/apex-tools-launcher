using ATL.Core.Class;
using CommunityToolkit.HighPerformance;
using RustyOptions;

namespace ApexFormat.IRTPC.V14.Class;

/// <summary>
/// Structure:
/// <br/>NameHash - <see cref="uint"/>
/// <br/>MajorVersion - <see cref="byte"/>
/// <br/>MinorVersion - <see cref="ushort"/>
/// <br/>PropertyCount - <see cref="ushort"/>
/// </summary>
public class IrtpcV14ContainerHeader : ISizeOf
{
    public uint NameHash = 0;
    public byte MajorVersion = 0;
    public ushort MinorVersion = 0;
    public ushort PropertyCount = 0;

    public static uint SizeOf()
    {
        return sizeof(uint) + // NameHash
               sizeof(byte) + // MajorVersion
               sizeof(ushort) + // MinorVersion
               sizeof(ushort); // PropertyCount
    }
}

public static class IrtpcV14ContainerHeaderExtensions
{
    public static Option<IrtpcV14ContainerHeader> ReadIrtpcV14ContainerHeader(this Stream stream)
    {
        if (stream.Length - stream.Position < IrtpcV14ContainerHeader.SizeOf())
        {
            return Option<IrtpcV14ContainerHeader>.None;
        }

        var result = new IrtpcV14ContainerHeader
        {
            NameHash = stream.Read<uint>(),
            MajorVersion = stream.Read<byte>(),
            MinorVersion = stream.Read<ushort>(),
            PropertyCount = stream.Read<ushort>(),
        };

        return Option.Some(result);
    }
}