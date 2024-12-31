using System.Text.Json.Serialization;

namespace ApexToolsLauncher.Core.Config.GUI;

public enum ModType
{
    VfsFile = 0,
    VfsArchive = 1,
    Dll = 2
}

public class ModConfig : ICloneable
{
    [JsonPropertyName("title")]
    public string Title { get; set; } = "Title";
    
    [JsonPropertyName("description")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string Description { get; set; } = "";

    [JsonPropertyName("type")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ModType Type { get; set; } = ModType.VfsFile;
    
    [JsonIgnore]
    public Dictionary<string, ModContentConfig> Versions { get; set; } = [];
    
    // ReSharper disable once UnusedMember.Global
    [JsonPropertyName("versions")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, ModContentConfig>? JsonVersions
    {
        get => Versions.Count != 0 ? Versions : null;
        set => Versions = value ?? [];
    }
    
    [JsonIgnore]
    public List<DependencyConfig> Dependencies { get; set; } = [];
    
    // ReSharper disable once UnusedMember.Global
    [JsonPropertyName("dependencies")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<DependencyConfig>? JsonDependencies
    {
        get => Dependencies.Count != 0 ? Dependencies : null;
        set => Dependencies = value ?? [];
    }

    protected bool Equals(ModConfig other)
    {
        return Title == other.Title &&
               Description == other.Description &&
               Type == other.Type &&
               Versions.Equals(other.Versions) &&
               Dependencies.Equals(other.Dependencies);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((ModConfig)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Title, Description, (int)Type, Versions, Dependencies);
    }

    public object Clone()
    {
        return MemberwiseClone();
    }
}
