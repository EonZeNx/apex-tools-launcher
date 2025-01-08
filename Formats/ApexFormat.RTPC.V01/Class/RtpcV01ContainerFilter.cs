using ApexFormat.RTPC.V01.Enum;
using ApexToolsLauncher.Core.Hash;

namespace ApexFormat.RTPC.V01.Class;

public interface IRtpcV01ContainerFilter
{
    public bool Match(RtpcV01Container container);
}

public class Filter : IRtpcV01ContainerFilter
{
    public string Name { get; set; }
    public IRtpcV01ContainerFilter[] SubFilters { get; set; }
    
    public Filter(string name, IRtpcV01ContainerFilter[]? subFilters = null)
    {
        Name = name;
        SubFilters = subFilters ?? [];
    }

    public virtual bool Match(RtpcV01Container container)
    {
        return container.Properties.Any(p => Name.HashJenkins() == p.NameHash);
    }
}

public class FilterString : Filter
{
    public string? Value { get; set; }
    
    public FilterString(string? value, string name, IRtpcV01ContainerFilter[]? subFilters = null) : base(name, subFilters)
    {
        Value = value;
    }

    public override bool Match(RtpcV01Container container)
    {
        var result = false;
        
        foreach (var property in container.Properties)
        {
            if (Name.HashJenkins() != property.NameHash) continue;
            if (property.DeferredData is null) continue;
            if (property.VariantType != ERtpcV01VariantType.String) continue;

            result = (string) property.DeferredData == Value;
        }
        
        return result;
    }
}
