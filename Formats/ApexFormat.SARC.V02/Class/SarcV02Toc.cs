using RustyOptions;

namespace ApexFormat.SARC.V02.Class;

/// <summary>
/// Structure:
/// <br/>MagicLength - <see cref="uint"/>
/// <br/>Magic - <see cref="uint"/>
/// <br/>Version - <see cref="uint"/>
/// <br/>Size - <see cref="uint"/>
/// </summary>
public class SarcV02Toc
{
    public SarcV02Entry[] Entries = [];
}

public static class SarcV02TocLibrary
{
    public static Option<SarcV02Toc> ReadSarcV02Toc(this Stream stream)
    {
        var result = new SarcV02Toc();
        
        var archiveEntries = new List<SarcV02Entry>();
        while (true)
        {
            var optionArchiveEntry = stream.ReadSarcV02Entry();
            if (!optionArchiveEntry.IsSome(out var archiveEntry))
                break;
            
            archiveEntries.Add(archiveEntry);
            if (stream.Position >= stream.Length)
            {
                break;
            }
        }
        
        result.Entries = archiveEntries.ToArray();

        return Option.Some(result);
    }
}