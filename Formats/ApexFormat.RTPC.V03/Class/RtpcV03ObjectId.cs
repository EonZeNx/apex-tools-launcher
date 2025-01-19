using System.Globalization;
using CommunityToolkit.HighPerformance;

namespace ApexFormat.RTPC.V03.Class;

public class RtpcV03ObjectId
{
    public ushort First = 0;
    public ushort Second = 0;
    public ushort Third = 0;
    public ushort Data = 0;

    public override string ToString()
    {
        return $"{this.ToUInt64():X016}";
    }
}

public static class RtpcV03ObjectIdLibrary
{
    public static RtpcV03ObjectId FromUInt64(ulong value)
    {
        var oid = new RtpcV03ObjectId
        {
            First = (ushort) ((value >> 48) & 0xFFFF),
            Second = (ushort) ((value >> 32) & 0xFFFF),
            Third = (ushort) ((value >> 16) & 0xFFFF),
            Data = (ushort) (value & 0xFFFF)
        };
        
        return oid;
    }
    
    public static ulong ToUInt64(this RtpcV03ObjectId oid)
    {
        var result = oid.Data | ((oid.Third | (oid.Second | ((ulong) oid.First) << 16) << 16) << 16);
        
        return result;
    }
    
    public static RtpcV03ObjectId FromString(string s)
    {
        var value = ulong.Parse(s, NumberStyles.HexNumber);
        var oid = FromUInt64(value);

        return oid;
    }
    
    public static RtpcV03ObjectId ReadRtpcV01ObjectId(this Stream stream)
    {
        var value = stream.Read<ulong>();
        var oid = FromUInt64(value);
        
        return oid;
    }
}