using CommunityToolkit.HighPerformance;

namespace ApexFormat.RTPC.V01.Class;

public class RtpcV01ObjectNameHash
{
    public ushort First = 0;
    public ushort Second = 0;
    public ushort Third = 0;
}

public class RtpcV01ObjectId : RtpcV01ObjectNameHash
{
    public ushort UserData = 0;
}

public static class RtpcV01ObjectIdExtensions
{
    public static ulong Hex(this RtpcV01ObjectId oid)
    {
        var result = (ulong) oid.First << 0x10;
        result = oid.Second | result << 0x10;
        result = oid.Third | result << 0x10;
        result = oid.UserData | result << 0x10;
        
        return result;
    }
    
    public static string String(this RtpcV01ObjectId oid)
    {
        var result = $"{oid.Hex():X016}";

        return result;
    }
    
    public static RtpcV01ObjectId ReadRtpcV01ObjectId(this Stream stream)
    {
        var result = new RtpcV01ObjectId
        {
            First = stream.Read<ushort>(),
            Second = stream.Read<ushort>(),
            Third = stream.Read<ushort>(),
            UserData = stream.Read<ushort>()
        };
        
        return result;
    }
}