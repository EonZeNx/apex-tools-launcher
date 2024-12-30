using ApexFormat.IC.V01.Enum;
using ApexToolsLauncher.Core.Class;
using CommunityToolkit.HighPerformance;
using RustyOptions;

namespace ApexFormat.IC.V01.Class;

/// <summary>
/// Structure:
/// <br/>Type - <see cref="EIcV01ContainerType"/>
/// <br/>Count - <see cref="ushort"/>
/// </summary>
public class IcV01Collection : ISizeOf
{
    public uint NameHash = 0;
    public byte Count = 0;
    public IcV01Container[] Containers = [];

    public static uint SizeOf()
    {
        return sizeof(EIcV01ContainerType) + // Type
               sizeof(ushort); // Count
    }
}

public static class IcV01CollectionExtensions
{
    public static Option<IcV01Collection> ReadIcV01Collection(this Stream stream)
    {
        if (stream.Length - stream.Position < IcV01Collection.SizeOf())
        {
            return Option<IcV01Collection>.None;
        }

        var result = new IcV01Collection
        {
            NameHash = stream.Read<uint>(),
            Count = stream.Read<byte>(),
        };
        
        result.Containers = new IcV01Container[result.Count];
        for (var i = 0; i < result.Count; i++)
        {
            var optionContainer = stream.ReadIcV01Container();
            if (optionContainer.IsSome(out var container))
                result.Containers[i] = container;
        }

        return Option.Some(result);
    }
}