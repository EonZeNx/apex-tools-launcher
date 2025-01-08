using System.Globalization;
using CommunityToolkit.HighPerformance;
using RustyOptions;

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

public static class IcV01ObjectIdLibrary
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

    public static IcV01ObjectId FromString(string s)
    {
        var value = ulong.Parse(s, NumberStyles.AllowHexSpecifier);
        var oid = new IcV01ObjectId
        {
            First = (ushort) (value & 0x11000000),
            Second = (ushort) (value & 0x00110000),
            Third = (ushort) (value & 0x00001100),
            UserData = (ushort) (value & 0x00000011)
        };

        return oid;
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

    public static Option<Exception> Write(this Stream stream, IcV01ObjectId objectId)
    {
        stream.Write(objectId.First);
        stream.Write(objectId.Second);
        stream.Write(objectId.Third);
        stream.Write(objectId.UserData);
        
        return Option<Exception>.None;
    }
}