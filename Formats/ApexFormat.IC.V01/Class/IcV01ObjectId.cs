using CommunityToolkit.HighPerformance;

namespace ApexFormat.IC.V01.Class;

public class IcV01ObjectNameHash
{
    public ushort First = 0;
    public ushort Second = 0;
    public ushort Third = 0;
}

public class IcV01ObjectId : IcV01ObjectNameHash
{
    public ushort UserData = 0;
}

public static class IcV01ObjectIdExtensions
{
    public static ulong Hex(this IcV01ObjectId oid)
    {
        var result = (ulong) oid.First << 0x10;
        result = oid.Second | result << 0x10;
        result = oid.Third | result << 0x10;
        result = oid.UserData | result << 0x10;
        
        return result;
    }
    
    public static string String(this IcV01ObjectId oid)
    {
        var result = $"{oid.Hex():X016}";

        return result;
    }
    
    public static IcV01ObjectId ReadIcV01ObjectId(this Stream stream)
    {
        var result = new IcV01ObjectId
        {
            First = stream.Read<ushort>(),
            Second = stream.Read<ushort>(),
            Third = stream.Read<ushort>(),
            UserData = stream.Read<ushort>()
        };
        
        return result;
    }
}