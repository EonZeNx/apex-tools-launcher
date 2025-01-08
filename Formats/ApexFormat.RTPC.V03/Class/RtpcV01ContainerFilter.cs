using ApexFormat.RTPC.V03.Enum;
using ApexToolsLauncher.Core.Hash;

namespace ApexFormat.RTPC.V03.Class;

public interface IRtpcV03Filter
{
    public bool MatchContainer(RtpcV03Container container);
    public bool MatchProperty(RtpcV03Variant property, bool subMatch = false);
    public bool SubMatchProperty(RtpcV03Variant property);
}

public class RtpcV03Filter : IRtpcV03Filter
{
    public string Name { get; set; }
    public uint NameHash { get; init; }
    public IRtpcV03Filter[] SubFilters { get; set; }
    
    public RtpcV03Filter(string name, IRtpcV03Filter[]? subFilters = null)
    {
        Name = name;
        NameHash = name.Jenkins();
        SubFilters = subFilters ?? [];
    }

    public virtual bool MatchContainer(RtpcV03Container container)
    {
        return container.Properties.Any(p => MatchProperty(p));
    }

    public virtual bool MatchProperty(RtpcV03Variant property, bool subMatch = false)
    {
        var result = NameHash == property.NameHash;
        if (!result && subMatch)
        {
            result |= SubMatchProperty(property);
        }

        return result;
    }

    public virtual bool SubMatchProperty(RtpcV03Variant property)
    {
        return SubFilters.Any(f => f.MatchProperty(property));
    }

    public override string ToString()
    {
        return $"{Name} with {SubFilters.Length} subfilters";
    }
}

public class RtpcV03FilterString : IRtpcV03Filter
{
    public string Name { get; set; }
    public uint NameHash { get; init; }
    public string Value { get; set; }
    public IRtpcV03Filter[] SubFilters { get; set; }
    
    public RtpcV03FilterString(string name, string value, IRtpcV03Filter[]? subFilters = null)
    {
        Name = name;
        NameHash = name.Jenkins();
        Value = value;
        SubFilters = subFilters ?? [];
    }

    public virtual bool MatchContainer(RtpcV03Container container)
    {
        return container.Properties.Any(property => MatchProperty(property));
    }

    public virtual bool MatchProperty(RtpcV03Variant property, bool subMatch = false)
    {
        if (NameHash == property.NameHash &&
            property.DeferredData is not null &&
            property.VariantType == ERtpcV03VariantType.String)
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

    public virtual bool SubMatchProperty(RtpcV03Variant property)
    {
        return SubFilters.Any(f => f.MatchProperty(property));
    }

    public override string ToString()
    {
        return $"{Name}: \"{Value}\" with {SubFilters.Length} subfilters";
    }
}
