using CommunityToolkit.HighPerformance;

namespace ApexFormat.RTPC.V03.Class;

public class RtpcV03ObjectNameHash
{
    public ushort First = 0;
    public ushort Second = 0;
    public ushort Third = 0;
}

public class RtpcV03ObjectId : RtpcV03ObjectNameHash
{
    public ushort UserData = 0;
}

public static class RtpcV03ObjectIdExtensions
{
    public static ulong Hex(this RtpcV03ObjectId oid)
    {
        var result = (ulong) oid.First << 0x10;
        result = oid.Second | result << 0x10;
        result = oid.Third | result << 0x10;
        result = oid.UserData | result << 0x10;
        
        return result;
    }
    
    public static string String(this RtpcV03ObjectId oid)
    {
        var result = $"{oid.Hex():X016}";

        return result;
    }
    
    public static RtpcV03ObjectId ReadRtpcV01ObjectId(this Stream stream)
    {
        var result = new RtpcV03ObjectId
        {
            First = stream.Read<ushort>(),
            Second = stream.Read<ushort>(),
            Third = stream.Read<ushort>(),
            UserData = stream.Read<ushort>()
        };
        
        return result;
    }
}