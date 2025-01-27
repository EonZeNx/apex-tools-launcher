using CommunityToolkit.HighPerformance;
using RustyOptions;

namespace ApexFormat.ADF.V04.Class;

public class AdfV04InstanceInfo
{
    public uint NameHash = 0;
    public uint TypeHash = 0;
    public uint InstanceOffset = 0;
    public uint InstanceSize = 0;
    public ulong NameIndex = 0;
    
    public string Name = "";
}

public static class AdfV04InstanceInfoExtensions
{
    public static Option<AdfV04InstanceInfo> ReadAdfV04InstanceInfo(this Stream stream)
    {
        if (stream.Length - stream.Position < AdfV04InstanceLibrary.SizeOf)
        {
            return Option<AdfV04InstanceInfo>.None;
        }

        var result = new AdfV04InstanceInfo
        {
            NameHash = stream.Read<uint>(),
            TypeHash = stream.Read<uint>(),
            InstanceOffset = stream.Read<uint>(),
            InstanceSize = stream.Read<uint>(),
            NameIndex = stream.Read<ulong>()
        };

        return Option.Some(result);
    }
}
