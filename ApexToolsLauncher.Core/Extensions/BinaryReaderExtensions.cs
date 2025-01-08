using System.Text;

namespace ApexToolsLauncher.Core.Extensions;

public static class BinaryReaderExtensions
{
    public static string ReadStringOfLength(this BinaryReader br, uint length)
    {
        var fullString = Encoding.UTF8.GetString(br.ReadBytes((int) length));

        return fullString;
    }
    
    public static string ReadStringOfLength(this BinaryReader br, int length)
    {
        var fullString = Encoding.UTF8.GetString(br.ReadBytes(length));

        return fullString;
    }
    
    public static string ReadStringLengthPrefix(this BinaryReader br)
    {
        var stringLength = br.ReadInt32();
        var fullString = Encoding.UTF8.GetString(br.ReadBytes(stringLength));

        return fullString;
    }
    
    public static string ReadStringZ(this BinaryReader br)
    {
        var fullString = "";
        var character = "";
            
        while (character != "\0")
        {
            fullString += character;
            character = Encoding.UTF8.GetString(br.ReadBytes(1));
        }

        return fullString;
    }
}