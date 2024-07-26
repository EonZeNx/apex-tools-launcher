using ATL.Core.Class;
using CommunityToolkit.HighPerformance;
using RustyOptions;

namespace ApexFormat.RTPC.V0104;

/// <summary>
/// Structure:
/// <br/>NameHash - <see cref="uint"/>
/// <br/>MajorVersion - <see cref="byte"/>
/// <br/>MinorVersion - <see cref="ushort"/>
/// <br/>PropertyCount - <see cref="ushort"/>
/// </summary>
public class RtpcV0104ContainerHeader : ISizeOf
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

public static class RtpcV0104ContainerHeaderExtensions
{
    public static Option<RtpcV0104ContainerHeader> ReadRtpcV0104ContainerHeader(this Stream stream)
    {
        if (stream.Length - stream.Position < RtpcV0104ContainerHeader.SizeOf())
        {
            return Option<RtpcV0104ContainerHeader>.None;
        }

        var result = new RtpcV0104ContainerHeader
        {
            NameHash = stream.Read<uint>(),
            MajorVersion = stream.Read<byte>(),
            MinorVersion = stream.Read<ushort>(),
            PropertyCount = stream.Read<ushort>(),
        };

        return Option.Some(result);
    }
}