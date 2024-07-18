using System.Text;
using ATL.Core.Extensions;

namespace ApexFormat.AAF.V01;

public static class AafV01HeaderConstants
{
    public const uint Magic = 0x00464141; // "_FAA"
    public const uint Version = 0x01;
    public const string Magic2 = "AVALANCHEARCHIVEFORMATISCOOL";
}

public class AafV01Header
{
    public uint Magic = AafV01HeaderConstants.Magic;
    public uint Version = AafV01HeaderConstants.Version;
    public string Magic2 = AafV01HeaderConstants.Magic2;
    public uint TotalUnpackedSize = 0;
    public uint RequiredUnpackBufferSize = 0;
    public uint NumChunks = 0;
}

public static class AafV01HeaderExtensions
{
    public static AafV01Header ReadAafV01Header(this Stream stream)
    {
        using var br = new BinaryReader(stream, Encoding.UTF8, true);

        var header = new AafV01Header
        {
            Magic = br.ReadUInt32(),
            Version = br.ReadUInt32(),
            Magic2 = br.ReadStringOfLength(AafV01HeaderConstants.Magic2.Length),
            TotalUnpackedSize = br.ReadUInt32(),
            RequiredUnpackBufferSize = br.ReadUInt32(),
            NumChunks = br.ReadUInt32()
        };

        return header;
    }
}