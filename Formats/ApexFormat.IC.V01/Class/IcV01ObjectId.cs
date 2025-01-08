using System.Globalization;
using CommunityToolkit.HighPerformance;
using RustyOptions;

namespace ApexFormat.IC.V01.Class;

public class IcV01ObjectId
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

public static class IcV01ObjectIdLibrary
{
    public static IcV01ObjectId FromUInt64(ulong value)
    {
        var oid = new IcV01ObjectId
        {
            First = (ushort) ((value >> 48) & 0xFFFF),
            Second = (ushort) ((value >> 32) & 0xFFFF),
            Third = (ushort) ((value >> 16) & 0xFFFF),
            Data = (ushort) (value & 0xFFFF)
        };
        
        return oid;
    }
    
    public static ulong ToUInt64(this IcV01ObjectId oid, bool reverse = false)
    {
        var result = oid.Data | ((oid.Third | (oid.Second | ((ulong) oid.First) << 16) << 16) << 16);
        
        return result;
    }
    
    public static IcV01ObjectId FromString(string s)
    {
        var value = ulong.Parse(s, NumberStyles.HexNumber);
        var oid = FromUInt64(value);

        return oid;
    }
    
    public static IcV01ObjectId ReadIcV01ObjectId(this Stream stream)
    {
        var value = stream.Read<ulong>();
        var oid = FromUInt64(value);
        
        return oid;
    }

    public static Option<Exception> Write(this Stream stream, IcV01ObjectId oid)
    {
        stream.Write(oid.ToUInt64());
        
        return Option<Exception>.None;
    }
}