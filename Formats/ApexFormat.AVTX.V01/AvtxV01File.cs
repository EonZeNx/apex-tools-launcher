using ApexFormat.AVTX.V01.Class;

namespace ApexFormat.AVTX.V01;

public class AvtxV01File
{
    public AvtxV01Header Header = new();

    public void ReadAvtx(Stream stream)
    {
        var optionTextureEntry = Header.GetBestEntry();
        if (!optionTextureEntry.IsSome(out var textureEntry))
            return;
    }
}
