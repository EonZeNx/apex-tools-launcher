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
}

public class BitStream
{
    protected Stream _stream;
    protected byte CurrentByte;
    protected byte Index;
    protected long Position;

    public BitStream(Stream stream)
    {
        _stream = stream;
        CurrentByte = 0;
        Index = 0;
        Position = 0;
    }

    public byte ReadBit()
    {
        if (Index >= sizeof(byte) || Position == 0 || Position != _stream.Position)
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