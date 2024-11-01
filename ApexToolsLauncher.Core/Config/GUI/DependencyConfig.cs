using System.Text.Json.Serialization;

namespace ApexToolsLauncher.Core.Config.GUI;

public enum DependencyType
{
    [JsonPropertyName("archive")]
    Archive,
    [JsonPropertyName("loader")]
    Loader,
    [JsonPropertyName("mod")]
    Mod
}

public class DependencyConfig
{
    [JsonPropertyName("type")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public DependencyType Type { get; set; } = DependencyType.Archive;
    
    [JsonPropertyName("targets")]
    public List<string> Targets { get; set; } = [];
}
