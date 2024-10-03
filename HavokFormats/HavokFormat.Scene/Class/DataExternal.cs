using ATL.Core.Class;
using CommunityToolkit.HighPerformance;
using RustyOptions;

namespace HavokFormat.Scene.Class;

/// <summary>
/// A data position descriptor aimed at a given section.
/// </summary>
public class DataExternal : ISizeOf
{
    /// <summary>
    /// The data parent, in the current section.
    /// </summary>
    public int FromCompressed { get; set; }
    // TODO: replace with actual decompress
    public long From => FromCompressed;
    
    /// <summary>
    /// The section the data is in.
    /// </summary>
    public int Section { get; set; }
    
    /// <summary>
    /// The data position, in the given section.
    /// </summary>
    public int ToCompressed { get; set; }
    // TODO: replace with actual decompress
    public long To => ToCompressed;
    
    public static uint SizeOf()
    {
        return sizeof(int) + // From
               sizeof(int) + // Section
               sizeof(int); // To
    }
}

public static class DataExternalExtensions
{
    public static Option<DataExternal> ReadDataExternal(this Stream stream)
    {
        if (stream.Length - stream.Position < DataExternal.SizeOf())
        {
            return Option<DataExternal>.None;
        }

        var result = new DataExternal
        {
            FromCompressed = stream.Read<int>(),
            Section = stream.Read<int>(),
            ToCompressed = stream.Read<int>(),
        };

        return Option.Some(result);
    }
}