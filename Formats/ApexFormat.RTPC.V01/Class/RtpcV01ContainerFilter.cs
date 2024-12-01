using ApexFormat.RTPC.V01.Enum;
using ApexToolsLauncher.Core.Hash;

namespace ApexFormat.RTPC.V01.Class;

public interface IRtpcV01Filter
{
    public bool MatchContainer(RtpcV01Container container);
    public bool MatchProperty(RtpcV01Variant property, bool subMatch = false);
    public bool SubMatchProperty(RtpcV01Variant property);
}

public class RtpcV01Filter : IRtpcV01Filter
{
    public string Name { get; set; }
    public uint NameHash { get; init; }
    public IRtpcV01Filter[] SubFilters { get; set; }
    
    public RtpcV01Filter(string name, IRtpcV01Filter[]? subFilters = null)
    {
        Name = name;
        NameHash = name.Jenkins();
        SubFilters = subFilters ?? [];
    }

    public virtual bool MatchContainer(RtpcV01Container container)
    {
        return container.Properties.Any(p => MatchProperty(p));
    }

    public virtual bool MatchProperty(RtpcV01Variant property, bool subMatch = false)
    {
        var result = NameHash == property.NameHash;
        if (!result && subMatch)
        {
            result |= SubMatchProperty(property);
        }

        return result;
    }

    public virtual bool SubMatchProperty(RtpcV01Variant property)
    {
        return SubFilters.Any(f => f.MatchProperty(property));
    }

    public override string ToString()
    {
        return $"{Name} with {SubFilters.Length} subfilters";
    }
}

public class RtpcV01FilterString : IRtpcV01Filter
{
    public string Name { get; set; }
    public uint NameHash { get; init; }
    public string Value { get; set; }
    public IRtpcV01Filter[] SubFilters { get; set; }
    
    public RtpcV01FilterString(string name, string value, IRtpcV01Filter[]? subFilters = null)
    {
        Name = name;
        NameHash = name.Jenkins();
        Value = value;
        SubFilters = subFilters ?? [];
    }

    public virtual bool MatchContainer(RtpcV01Container container)
    {
        return container.Properties.Any(property => MatchProperty(property));
    }

    public virtual bool MatchProperty(RtpcV01Variant property, bool subMatch = false)
    {
        if (NameHash == property.NameHash &&
            property.DeferredData is not null &&
            property.VariantType == ERtpcV01VariantType.String)
        {
            var deferredData = ((string) property.DeferredData).Trim();
            if (deferredData.Equals(Value)) return true;
        }

        if (subMatch)
        {
            return SubFilters.Any(f => f.MatchProperty(property));
        }

        return false;
    }

    public virtual bool SubMatchProperty(RtpcV01Variant property)
    {
        return SubFilters.Any(f => f.MatchProperty(property));
    }

    public override string ToString()
    {
        return $"{Name}: \"{Value}\" with {SubFilters.Length} subfilters";
    }
}
