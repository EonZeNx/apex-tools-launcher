using ApexToolsLauncher.Core.Class;
using ApexToolsLauncher.Core.Extensions;
using CommunityToolkit.HighPerformance;
using RustyOptions;

namespace ApexFormat.AAF.V01;

public static class AafV01HeaderConstants
{
    public const uint Magic = 0x00464141; // "_FAA"
    public const uint Version = 0x01;
    public const string Comment = "AVALANCHEARCHIVEFORMATISCOOL";
}

/// <summary>
/// Structure:
/// <br/>Magic - <see cref="uint"/>
/// <br/>Version - <see cref="uint"/>
/// <br/>Comment - <see cref="string"/>
/// <br/>TotalUnpackedSize - <see cref="uint"/>
/// <br/>RequiredUnpackBufferSize - <see cref="uint"/>
/// <br/>ChunkCount - <see cref="uint"/>
/// </summary>
public class AafV01Header : ISizeOf
{
    public uint Magic = AafV01HeaderConstants.Magic;
    public uint Version = AafV01HeaderConstants.Version;
    public string Comment = AafV01HeaderConstants.Comment;
    public uint TotalUnpackedSize = 0;
    public uint RequiredUnpackBufferSize = 0;
    public uint ChunkCount = 0;

    public static uint SizeOf()
    {
        return (uint) (sizeof(uint) + // Magic
                       sizeof(uint) + // Version
                       AafV01HeaderConstants.Comment.Length + // Comment
                       sizeof(uint) + // TotalUnpackedSize
                       sizeof(uint) + // RequiredUnpackBufferSize
                       sizeof(uint)); // ChunkCount
    }
}

public static class AafV01HeaderExtensions
{
    public static Option<AafV01Header> ReadAafV01Header(this Stream stream)
    {
        if (stream.Length - stream.Position < AafV01Header.SizeOf())
        {
            return Option<AafV01Header>.None;
        }

        var result = new AafV01Header
        {
            Magic = stream.Read<uint>(),
            Version = stream.Read<uint>(),
            Comment = stream.ReadStringOfLength(AafV01HeaderConstants.Comment.Length),
            TotalUnpackedSize = stream.Read<uint>(),
            RequiredUnpackBufferSize = stream.Read<uint>(),
            ChunkCount = stream.Read<uint>()
        };
        
        if (result.Magic != AafV01HeaderConstants.Magic)
        {
            return Option<AafV01Header>.None;
        }

        if (result.Version != AafV01HeaderConstants.Version)
        {
            return Option<AafV01Header>.None;
        }

        return Option.Some(result);
    }
}