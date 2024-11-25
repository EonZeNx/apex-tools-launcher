using ApexFormat.RTPC.V01.Enum;
using ApexToolsLauncher.Core.Hash;

namespace ApexFormat.RTPC.V01.Class;

public interface IRtpcV01Filter
{
    public bool MatchContainer(RtpcV01Container container, bool useSubFilters = true);
    public bool MatchProperty(RtpcV01Variant property, bool useSubFilters = true);
}

public class RtpcV01Filter : IRtpcV01Filter
{
    public string Name { get; set; }
    public IRtpcV01Filter[] SubFilters { get; set; }
    
    public RtpcV01Filter(string name, IRtpcV01Filter[]? subFilters = null)
    {
        Name = name;
        SubFilters = subFilters ?? [];
    }

    public virtual bool MatchContainer(RtpcV01Container container, bool useSubFilters = true)
    {
        return container.Properties.Any(p => MatchProperty(p, useSubFilters));
    }

    public virtual bool MatchProperty(RtpcV01Variant property, bool useSubFilters = true)
    {
        var result = Name.HashJenkins() == property.NameHash;
        if (useSubFilters && SubFilters.Length > 0)
        {
            result |= SubFilters.Any(f => f.MatchProperty(property));
        }

        return result;
    }
}

public class RtpcV01FilterString : IRtpcV01Filter
{
    public string Name { get; set; }
    public string Value { get; set; }
    public IRtpcV01Filter[] SubFilters { get; set; }
    
    public RtpcV01FilterString(string name, string value, IRtpcV01Filter[]? subFilters = null)
    {
        Name = name;
        Value = value;
        SubFilters = subFilters ?? [];
    }

    public virtual bool MatchContainer(RtpcV01Container container, bool useSubFilters = true)
    {
        var result = false;
        
        foreach (var property in container.Properties)
        {
            if (Name.HashJenkins() != property.NameHash) continue;
            if (property.DeferredData is null) continue;
            if (property.VariantType != ERtpcV01VariantType.String) continue;

            result = ((string) property.DeferredData).Equals(Value);
        }
        
        if (useSubFilters && SubFilters.Length > 0)
        {
            result |= SubFilters.Any(f => f.MatchContainer(container));
        }
        
        return result;
    }

    public virtual bool MatchProperty(RtpcV01Variant property, bool useSubFilters = true)
    {
        if (Name.HashJenkins() != property.NameHash) return false;
        if (property.DeferredData is null) return false;
        if (property.VariantType != ERtpcV01VariantType.String) return false;

        var result = (string) property.DeferredData == Value;
        if (useSubFilters && SubFilters.Length > 0)
        {
            result |= SubFilters.Any(f => f.MatchProperty(property));
        }

        return result;
    }
}
