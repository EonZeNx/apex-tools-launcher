using System.Text.Json.Serialization;

namespace ATL.Core.Config.GUI;

public class LaunchOptionConfig
{
    [JsonPropertyName("title")]
    public string Title { get; set; } = "Title";
    
    [JsonPropertyName("description")]
    public string Description { get; set; } = "";
    
    [JsonPropertyName("arguments")]
    public string Arguments { get; set; } = "";
    
    [JsonIgnore]
    public Dictionary<string, GameArgumentConfig> ArgumentConfig { get; set; } = [];
    
    // ReSharper disable once UnusedMember.Global
    [JsonPropertyName("argument_config")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, GameArgumentConfig>? JsonArgumentConfig
    {
        get => ArgumentConfig.Count != 0 ? ArgumentConfig : null;
        set => ArgumentConfig = value ?? [];
    }
}