using ATL.Core.Class;
using CommunityToolkit.HighPerformance;
using RustyOptions;

namespace ApexFormat.IRTPC.V14.Class;

public static class IrtpcV14HeaderConstants
{
    public const byte MajorVersion = 0x01;
    public const ushort MinorVersion = 0x04;
}

/// <summary>
/// Structure:
/// <br/>MajorVersion - <see cref="byte"/>
/// <br/>MinorVersion - <see cref="ushort"/>
/// <br/>ContainerCount - <see cref="ushort"/>
/// </summary>
public class IrtpcV14Header : ISizeOf
{
    public byte MajorVersion = IrtpcV14HeaderConstants.MajorVersion;
    public ushort MinorVersion = IrtpcV14HeaderConstants.MinorVersion;
    public ushort ContainerCount = 0;

    public static uint SizeOf()
    {
        return sizeof(byte) + // MajorVersion
               sizeof(ushort) + // MinorVersion
               sizeof(ushort); // ContainerCount
    }
}

public static class IrtpcV14HeaderExtensions
{
    public static Option<IrtpcV14Header> ReadIrtpcV14Header(this Stream stream)
    {
        if (stream.Length - stream.Position < IrtpcV14Header.SizeOf())
        {
            return Option<IrtpcV14Header>.None;
        }

        var result = new IrtpcV14Header
        {
            MajorVersion = (byte) stream.ReadByte(),
            MinorVersion = stream.Read<ushort>(),
            ContainerCount = stream.Read<ushort>(),
        };

        if (result.MajorVersion != IrtpcV14HeaderConstants.MajorVersion)
        {
            return Option<IrtpcV14Header>.None;
        }

        if (result.MinorVersion != IrtpcV14HeaderConstants.MinorVersion)
        {
            return Option<IrtpcV14Header>.None;
        }

        return Option.Some(result);
    }
}