using ApexFormat.ADF.V04.Enums;
using ApexToolsLauncher.Core.Class;
using ApexToolsLauncher.Core.Extensions;
using CommunityToolkit.HighPerformance;
using RustyOptions;

namespace ApexFormat.ADF.V04.Class;

/// <summary>
/// <remarks>
///  <list type="table">
///    <listheader>
///      <term>Property</term><description>Type</description>
///    </listheader>
///    <item>
///      <term><c>Magic</c></term><description><see cref="uint"/></description>
///    </item>
///    <item>
///      <term><c>Version</c></term><description><see cref="uint"/></description>
///    </item>
///    <item>
///      <term><c>InstanceCount</c></term><description><see cref="uint"/></description>
///    </item>
///    <item>
///      <term><c>InstanceOffset</c></term><description><see cref="uint"/></description>
///    </item>
///    <item>
///      <term><c>TypeCount</c></term><description><see cref="uint"/></description>
///    </item>
///    <item>
///      <term><c>TypeOffset</c></term><description><see cref="uint"/></description>
///    </item>
///    <item>
///      <term><c>StringHashCount</c></term><description><see cref="uint"/></description>
///    </item>
///    <item>
///      <term><c>StringHashOffset</c></term><description><see cref="uint"/></description>
///    </item>
///    <item>
///      <term><c>StringTableCount</c></term><description><see cref="uint"/></description>
///    </item>
///    <item>
///      <term><c>StringTableOffset</c></term><description><see cref="uint"/></description>
///    </item>
///    <item>
///      <term><c>FileSize</c></term><description><see cref="uint"/></description>
///    </item>
///    <item>
///      <term><c>MetaDataOffset</c></term><description><see cref="uint"/></description>
///    </item>
///    <item>
///      <term><c>Flags</c></term><description><see cref="EAdfV04HeaderFlags"/></description>
///    </item>
///    <item>
///      <term><c>IncludedLibraries</c></term><description><see cref="uint"/></description>
///    </item>
///    <item>
///      <term><c>Unknown01</c></term><description><see cref="uint"/></description>
///    </item>
///    <item>
///      <term><c>Unknown02</c></term><description><see cref="uint"/></description>
///    </item>
///    <item>
///      <term><c>Comment</c></term><description><see cref="string"/></description>
///    </item>
///  </list>
/// </remarks>
/// </summary>
public class AdfV04Header
{
    public uint Magic = AdfV04HeaderLibrary.Magic;
    public uint Version = AdfV04HeaderLibrary.Version;
    public uint InstanceCount = 0;
    public uint InstanceOffset = 0;
    public uint TypeCount = 0;
    public uint TypeOffset = 0;
    public uint StringHashCount = 0;
    public uint StringHashOffset = 0;
    public uint StringTableCount = 0;
    public uint StringTableOffset = 0;
    public uint FileSize = 0;
    public uint MetaDataOffset = 0;
    public EAdfV04HeaderFlags Flags = EAdfV04HeaderFlags.Default;
    public uint IncludedLibraries = 0;
    public uint Unknown01 = AdfV04HeaderLibrary.Unknown01;
    public uint Unknown02 = AdfV04HeaderLibrary.Unknown02;
    public string Comment = string.Empty;
}

public static class AdfV04HeaderLibrary
{
    public const uint SizeOf = sizeof(uint) // Magic
                               + sizeof(uint) // Version
                               + sizeof(uint) // InstanceCount
                               + sizeof(uint) // FirstInstanceOffset
                               + sizeof(uint) // TypeCount
                               + sizeof(uint) // TypeOffset
                               + sizeof(uint) // StringHashCount
                               + sizeof(uint) // StringHashOffset
                               + sizeof(uint) // StringTableCount
                               + sizeof(uint) // StringTableOffset
                               + sizeof(uint) // FileSize
                               + sizeof(uint) // MetaDataOffset
                               + sizeof(EAdfV04HeaderFlags) // Flags
                               + sizeof(uint) // IncludedLibraries
                               + sizeof(uint) // Unknown01
                               + sizeof(uint) // Unknown02
                               + sizeof(byte); // Min string length
    
    public const uint Magic = 0x41444620; // "ADF "
    public const uint Version = 0x04;
    public const uint Unknown01 = 32;
    public const uint Unknown02 = 32;
    
    public static Option<AdfV04Header> ReadAdfV04Header(this Stream stream)
    {
        if (!stream.CouldRead(SizeOf))
        {
            return Option<AdfV04Header>.None;
        }

        var result = new AdfV04Header
        {
            Magic = stream.Read<uint>(),
            Version = stream.Read<uint>(),
            InstanceCount = stream.Read<uint>(),
            InstanceOffset = stream.Read<uint>(),
            TypeCount = stream.Read<uint>(),
            TypeOffset = stream.Read<uint>(),
            StringHashCount = stream.Read<uint>(),
            StringHashOffset = stream.Read<uint>(),
            StringTableCount = stream.Read<uint>(),
            StringTableOffset = stream.Read<uint>(),
            FileSize = stream.Read<uint>(),
            MetaDataOffset = stream.Read<uint>(),
            Flags = stream.Read<EAdfV04HeaderFlags>(),
            IncludedLibraries = stream.Read<uint>(),
            Unknown01 = stream.Read<uint>(),
            Unknown02 = stream.Read<uint>(),
            Comment = stream.ReadStringZ(),
        };
        
        if (result.Magic != Magic)
        {
            return Option<AdfV04Header>.None;
        }

        if (result.Version != Version)
        {
            return Option<AdfV04Header>.None;
        }

        return Option.Some(result);
    }
}