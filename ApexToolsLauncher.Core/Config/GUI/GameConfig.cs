using System.Text.Json.Serialization;

namespace ApexToolsLauncher.Core.Config.GUI;

public class GameConfig
{
    [JsonPropertyName("title")]
    public string Title { get; set; } = "Title";
    
    [JsonPropertyName("steam_app_id")]
    public string SteamAppId { get; set; } = "SteamAppId";
    
    [JsonPropertyName("path")]
    public string Path { get; set; } = AppDomain.CurrentDomain.BaseDirectory;
    
    [JsonIgnore]
    public Dictionary<string, LaunchOptionConfig> LaunchOptions { get; set; } = [];
    
    // ReSharper disable once UnusedMember.Global
    [JsonPropertyName("launch_options")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, LaunchOptionConfig>? JsonLaunchOptions
    {
        get => LaunchOptions.Count != 0 ? LaunchOptions : null;
        set => LaunchOptions = value ?? [];
    }
}