using System.Text;
using System.Buffers;
using CommunityToolkit.HighPerformance;

namespace ATL.Core.Extensions;

public static class StreamExtensions
{
    public static void Align(this Stream stream, uint align)
    {
        stream.Seek(ByteExtensions.Align(stream.Position, align), SeekOrigin.Begin);
    }
    
    public static string ReadStringZ(this Stream stream)
    {
        var charList = new List<byte>();
        
        // Hardcoded sanity check
        while (charList.Count < 2000)
        {
            var newChar = stream.Read<byte>();
            if (newChar == '\0') break;
            
            charList.Add(newChar);
        }
        
        return Encoding.UTF8.GetString(charList.ToArray());
    }
    
    public static string ReadStringOfLength(this Stream stream, uint length) => stream.ReadStringOfLength((int)length);
    public static string ReadStringOfLength(this Stream stream, int length)
    {
        var rawString = new byte[length];
        stream.ReadExactly(rawString);
        
        var fullString = Encoding.UTF8.GetString(rawString);
    
        return fullString;
    }
    
    public static string ReadStringLengthPrefix(this Stream stream)
    {
        var stringLength = stream.Read<uint>();

        return stream.ReadStringOfLength(stringLength);
    }
    
    public static byte[] ReadBytes(this Stream stream, int count)
    {
        var buffer = new byte[count];
        var bytesRead = stream.Read(buffer);

        return buffer;
    }
    
    public static T[] ReadArray<T>(this Stream stream, int count) where T : unmanaged
    {
        var values = new T[count];
        for (var i = 0; i < count; i++)
        {
            values[i] = stream.Read<T>();
        }
        
        return values;
    }
    
    public static T[] ReadArrayLengthPrefix<T>(this Stream stream) where T : unmanaged
    {
        return stream.ReadArray<T>(stream.Read<int>());
    }

    public static void CopyToLimit(this Stream stream, Stream destination, int count) => stream.CopyToLimit(destination, count, 81920);
    public static void CopyToLimit(this Stream stream, Stream destination, int count, int bufferSize)
    {
        var buffer = ArrayPool<byte>.Shared.Rent(bufferSize);
        try
        {
            int bytesRead;
            while (count > 0 && (bytesRead = stream.Read(buffer, 0, Math.Min(buffer.Length, count))) != 0)
            {
                destination.Write(buffer, 0, bytesRead);
                count -= bytesRead;
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }
}