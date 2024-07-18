using System.Text.Json.Serialization;
using ATL.Core.Libraries;

namespace ATL.Core.Config.GUI;

public class GameConfig
{
    [JsonPropertyName("selected_profile")]
    public string SelectedProfile { get; set; } = ConstantsLibrary.InvalidString;
    
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