using ApexToolsLauncher.Core.Class;
using CommunityToolkit.HighPerformance;
using RustyOptions;

namespace ApexFormat.ADF.V04.Class;

/// <summary>
/// Structure:
/// <br/>NameHash - <see cref="uint"/>
/// <br/>TypeHash - <see cref="uint"/>
/// <br/>PayloadOffset - <see cref="uint"/>
/// <br/>PayloadSize - <see cref="uint"/>
/// <br/>NameIndex - <see cref="ulong"/>
/// </summary>
public class AdfV04Instance : ISizeOf
{
    public uint NameHash      = 0;
    public uint TypeHash      = 0;
    public uint PayloadOffset = 0;
    public uint PayloadSize   = 0;
    public ulong NameIndex    = 0;

    public string Name { get; set; } = "EMPTY";
    
    public static uint SizeOf()
    {
        return sizeof(uint) + // NameHash
               sizeof(uint) + // TypeHash
               sizeof(uint) + // PayloadOffset
               sizeof(uint) + // PayloadSize
               sizeof(ulong); // NameIndex
    }
}

public static class AdfV04InstanceExtensions
{
    public static Option<AdfV04Instance> ReadAdfV04Instance(this Stream stream)
    {
        if (stream.Length - stream.Position < AdfV04Instance.SizeOf())
        {
            return Option<AdfV04Instance>.None;
        }

        var result = new AdfV04Instance
        {
            NameHash = stream.Read<uint>(),
            TypeHash = stream.Read<uint>(),
            PayloadOffset = stream.Read<uint>(),
            PayloadSize = stream.Read<uint>(),
            NameIndex = stream.Read<ulong>()
        };

        return Option.Some(result);
    }
}