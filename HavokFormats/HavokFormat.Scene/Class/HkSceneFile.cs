namespace HavokFormat.Scene.Class;

public class HkSceneFile
{
    protected PointerNameGenerator PointerNameGenerator = new();
    
    public HkSceneHeader Header = new();
    public HkSceneSection ClassNameSection = new();
    public HkSceneSection TypesSection = new();
    public HkSceneSection DataSection = new();
    public HkClassName[] ClassNames = [];

    public void Read(Stream stream)
    {
        var optionHeader = stream.ReadHkSceneHeader();
        if (!optionHeader.IsSome(out var header))
            return;

        Header = header;
        
        var optionClassNameSection = stream.ReadHkSceneSection();
        if (!optionClassNameSection.IsSome(out var classNameSection))
            return;

        ClassNameSection = classNameSection;
        
        var optionTypesSection = stream.ReadHkSceneSection();
        if (!optionTypesSection.IsSome(out var typesSection))
            return;

        TypesSection = typesSection;
        
        var optionDataSection = stream.ReadHkSceneSection();
        if (!optionDataSection.IsSome(out var dataSection))
            return;

        DataSection = dataSection;

        stream.Seek(ClassNameSection.Offset, SeekOrigin.Begin);
        var classNames = new List<HkClassName>();
        while (stream.Position + HkClassName.SizeOf() < ClassNameSection.Offset + ClassNameSection.DataEndOffset)
        {
            var optionClassName = stream.ReadHkClassName();
            if (!optionClassName.IsSome(out var className))
                break;
            
            classNames.Add(className);
        }

        ClassNames = classNames.ToArray();
        
        // TODO: Data3 / PointerNameGenerator here
    }
}
