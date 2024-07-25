using RustyOptions;

namespace ApexFormat.ADF.V04.Class;

public class AdfV04InstanceInfo
{
    public uint NameHash = 0;
    public uint TypeHash = 0;
    public string Name = "";
    public uint InstanceOffset = 0;
    public uint InstanceSize = 0;
}

public static class AdfV04InstanceInfoExtensions
{
    public static Option<AdfV04InstanceInfo> ReadAdfV04InstanceInfo(this Stream stream)
    {
        if (stream.Length - stream.Position < AdfV04Instance.SizeOf())
        {
            return Option<AdfV04InstanceInfo>.None;
        }

        var optionInstance = stream.ReadAdfV04Instance();
        if (!optionInstance.IsSome(out var instance))
            return Option<AdfV04InstanceInfo>.None;

        var result = new AdfV04InstanceInfo
        {
            NameHash = instance.NameHash,
            TypeHash = instance.TypeHash
        };
        
        

        return Option.Some(result);
    }
}
