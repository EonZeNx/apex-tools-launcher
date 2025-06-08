using System.Reflection;

namespace ApexToolsLauncher.Core.Extensions;

public static class HackyExtensions
{
    // todo: figure out if this just fills up forever after dispose
    
    public static Dictionary<Stream, BitStream> BitStreams = new();

    public static byte ReadBit(this Stream stream)
    {
        if (!BitStreams.ContainsKey(stream))
            BitStreams.Add(stream, new BitStream(stream));
        
        return BitStreams[stream].ReadBit();
    }

    public static long BitStreamPosition(this Stream stream)
    {
        if (!BitStreams.TryGetValue(stream, out var bitStream))
            return -1;
        
        return bitStream.Position - 1;
    }

    public static void CleanBitStreams(this Stream stream)
    {
        BitStreams = BitStreams.Where((kvp) => !kvp.Key.IsDisposed()).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }
    
    
    public static bool IsDisposed(this IDisposable? obj)
    {
        /*
         TIM C: This hacky code is because MSFT does not provide a standard way to interrogate if an object is disposed or not.
            I wrote this based upon streams, but it should work for many other types of MSFT objects (maybe).
        */
        if (obj == null) { return true; }

        var objType = obj.GetType();
        //var foo = new System.IO.BufferedStream();

        // the _disposed pattern should catch a lot of msft objects.... hopefully
        var isDisposedField = objType.GetField("_disposed", BindingFlags.NonPublic | BindingFlags.Instance) ??
                              objType.GetField("disposed", BindingFlags.NonPublic | BindingFlags.Instance);

        if (isDisposedField != null) { return Convert.ToBoolean(isDisposedField.GetValue(obj)); }

        isDisposedField = objType.GetField("_isOpen", BindingFlags.NonPublic | BindingFlags.Instance);

        if (isDisposedField != null) { return !Convert.ToBoolean(isDisposedField.GetValue(obj)); }

        // System.IO.FileStream
        var strategyField = objType.GetField("_strategy", BindingFlags.NonPublic | BindingFlags.Instance);
        if (strategyField != null)
        {
            var strategy = strategyField.GetValue(obj);
            var isClosedField = strategy?.GetType().GetProperty("IsClosed", BindingFlags.NonPublic | BindingFlags.Instance);
            if (isClosedField != null) { return Convert.ToBoolean(isClosedField.GetValue(strategy)); }
        }

        // other streams that use this pattern to determine if they are disposed
        if (obj is Stream stream) { return !stream.CanRead && !stream.CanWrite; }

        return false;
    }
}

public class BitStream
{
    protected Stream _stream;
    protected byte CurrentByte;
    protected byte Index;
    public long Position { get; protected set; }

    public BitStream(Stream stream)
    {
        _stream = stream;
        CurrentByte = 0;
        Index = 0;
        Position = 0;
    }

    public byte ReadBit()
    {
        if (Index >= 8 || Position == 0 || Position != _stream.Position)
        {
            CurrentByte = (byte) _stream.ReadByte();
            Position = _stream.Position;
            Index = 0;
        }
        
        var value = (byte) ((CurrentByte >> Index) & 1);
        Index += 1;
        
        return value;
    }
}