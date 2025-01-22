using System.Text;
using System.Buffers;
using System.Runtime.CompilerServices;
using ApexToolsLauncher.Core.Class;
using CommunityToolkit.HighPerformance;
using RustyOptions;

namespace ApexToolsLauncher.Core.Extensions;

public static class StreamExtensions
{
    public static void Align(this Stream stream, int align, byte fill = 0x50) => stream.Align((uint) align, fill);
    
    public static void Align(this Stream stream, uint align, byte fill = 0x50)
    {
        var position = stream.Position;
        var alignment = ByteExtensions.Align(stream.Position, align);
        var relativeAlignment = alignment - position;

        var bytes = new byte[relativeAlignment];
        Array.Fill(bytes, fill);
        
        stream.Write(bytes);
    }
    
    public static string ReadStringZ(this Stream stream, int maxLength = 2048)
    {
        var charList = new List<byte>();
        
        while (charList.Count < maxLength)
        {
            if (stream.Position == stream.Length)
                break;
            
            var newChar = stream.Read<byte>();
            if (newChar == '\0')
                break;
            
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
    
    public static T[] ReadArray<T>(this Stream stream, int count)
        where T : unmanaged
    {
        var values = new T[count];
        for (var i = 0; i < count; i++)
        {
            values[i] = stream.Read<T>();
        }
        
        return values;
    }
    
    public static T[] ReadArrayLengthPrefix<T>(this Stream stream)
        where T : unmanaged
    {
        return stream.ReadArray<T>(stream.Read<int>());
    }

    public static string[] ReadArrayStringZ(this Stream stream, int count)
    {
        var values = new string[count];
        for (var i = 0; i < count; i++)
        {
            values[i] = stream.ReadStringZ();
        }
        
        return values;
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

    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe bool CouldRead<T>(this Stream stream)
        where T : unmanaged
    {
        return stream.Position + sizeof(T) <= stream.Length;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool CouldRead(this Stream stream, int size)
    {
        return stream.Position + size <= stream.Length;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool CouldRead(this Stream stream, uint size)
    {
        return stream.Position + size <= stream.Length;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<T, Exception> ReadResult<T>(this Stream stream, EEndian endian = EEndian.Little)
        where T : unmanaged
    {
        return stream.CouldRead<T>()
            ? Result.OkExn(stream.ReadEndian<T>(endian))
            : Result.Err<T>(new EndOfStreamException($"sizeof {nameof(T)} would exceed stream length"));
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe T ReadEndian<T>(this Stream stream, EEndian endian = EEndian.Little)
        where T : unmanaged
    {
        T result;

        var span = new Span<byte>(&result, sizeof(T));
        stream.ReadExactly(span);

        if (endian == EEndian.Big)
        {
            span.Reverse();
        }

        return result;
    }
}