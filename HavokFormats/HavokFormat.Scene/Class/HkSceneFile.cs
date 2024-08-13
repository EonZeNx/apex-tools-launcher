namespace HavokFormat.Scene.Class;

public class HkSceneFile
{
    public HkSceneHeader Header = new();
    public HkSceneSection Section = new();

    public void Read(Stream stream)
    {
        var optionHeader = stream.ReadHkSceneHeader();
        if (!optionHeader.IsSome(out var header))
        {
            return;
        }

        Header = header;
        
        var optionSection = stream.ReadHkSceneSection();
        if (!optionSection.IsSome(out var section))
        {
            return;
        }

        Section = section;
    }
}
