using ApexToolsLauncher.Core.Extensions;
using CommunityToolkit.HighPerformance;
using RustyOptions;

namespace ApexFormat.AAF.V01.Class;


/// <summary>
/// Structure:
/// <br/>Magic - <see cref="uint"/>
/// <br/>Version - <see cref="uint"/>
/// <br/>Comment - <see cref="string"/>
/// <br/>TotalUnpackedSize - <see cref="uint"/>
/// <br/>RequiredUnpackBufferSize - <see cref="uint"/>
/// <br/>ChunkCount - <see cref="uint"/>
/// </summary>
public class AafV01Header
{
    public uint Magic = AafV01HeaderLibrary.Magic;
    public uint Version = AafV01HeaderLibrary.Version;
    public string Comment = AafV01HeaderLibrary.Comment;
    public uint TotalUnpackedSize = 0;
    public uint RequiredUnpackBufferSize = 0;
    public uint ChunkCount = 0;
}

public static class AafV01HeaderLibrary
{
    public const int SizeOf = sizeof(uint) // Magic
                              + sizeof(uint) // Version
                              + CommentLength // Comment
                              + sizeof(uint) // TotalUnpackedSize
                              + sizeof(uint) // RequiredUnpackBufferSize
                              + sizeof(uint); // ChunkCount
    
    public const uint Magic = 0x00464141; // "_FAA"
    public const uint Version = 0x01;
    public const string Comment = "AVALANCHEARCHIVEFORMATISCOOL";
    public const int CommentLength = 28;
    
    public static Option<AafV01Header> ReadAafV01Header(this Stream stream)
    {
        if (stream.CouldRead(SizeOf))
        {
            return Option<AafV01Header>.None;
        }

        var result = new AafV01Header
        {
            Magic = stream.Read<uint>(),
            Version = stream.Read<uint>(),
            Comment = stream.ReadStringOfLength(Comment.Length),
            TotalUnpackedSize = stream.Read<uint>(),
            RequiredUnpackBufferSize = stream.Read<uint>(),
            ChunkCount = stream.Read<uint>()
        };
        
        if (result.Magic != Magic)
        {
            return Option<AafV01Header>.None;
        }

        if (result.Version != Version)
        {
            return Option<AafV01Header>.None;
        }

        return Option.Some(result);
    }
}