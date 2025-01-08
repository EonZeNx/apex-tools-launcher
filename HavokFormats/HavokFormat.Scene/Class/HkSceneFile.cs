namespace HavokFormat.Scene.Class;

public class HkSceneFile
{
    protected PointerNameGenerator PointerNameGenerator = new();
    
    public HkSceneHeader Header = new();
    public HkSceneSection ClassNameSection = new();
    public HkSceneSection TypesSection = new();
    public HkSceneSection DataSection = new();
    public Dictionary<long, HkClassName> PositionClassNameMap = [];

    public void Read(Stream stream)
    {
        {
            var optionHeader = stream.ReadHkSceneHeader();
            if (!optionHeader.IsSome(out var header))
                return;
            Header = header;
        }

        {
            var optionClassNameSection = stream.ReadHkSceneSection();
            if (!optionClassNameSection.IsSome(out var classNameSection))
                return;
            ClassNameSection = classNameSection;
        }

        {
            var optionTypesSection = stream.ReadHkSceneSection();
            if (!optionTypesSection.IsSome(out var typesSection))
                return;
            TypesSection = typesSection;
        }

        {
            var optionDataSection = stream.ReadHkSceneSection();
            if (!optionDataSection.IsSome(out var dataSection))
                return;
            DataSection = dataSection;
        }

        stream.Seek(ClassNameSection.Offset, SeekOrigin.Begin);
        while (stream.Position + HkClassName.SizeOf() < ClassNameSection.Offset + ClassNameSection.DataEndOffset)
        {
            var optionClassName = stream.ReadHkClassName();
            if (!optionClassName.IsSome(out var className))
                break;

            var classNamePosition = stream.Position - (className.Name.Length + 1) - ClassNameSection.Offset;
            PositionClassNameMap.Add(classNamePosition, className);
        }
        
        var data3Index = 0;
        while (true)
        {
            var optionClass = stream.ReadDataExternalFromIndex(DataSection, data3Index += 1);
            if (!optionClass.IsSome(out var dataClass))
                break;

            PointerNameGenerator.Add(dataClass.From);
        }
        
        data3Index = 0;
        while (true)
        {
            var optionClass = stream.ReadDataExternalFromIndex(DataSection, data3Index += 1);
            if (!optionClass.IsSome(out var dataClass))
                break;

            if (!PositionClassNameMap.TryGetValue(dataClass.To, out var hkClassName))
                continue;

            var className = hkClassName.Name;
            // TODO: descriptor
        }
    }
}
