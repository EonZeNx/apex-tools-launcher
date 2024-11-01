using System.Text.Json.Serialization;

namespace ApexToolsLauncher.Core.Config.GUI;

public class ProfileConfig
{
    [JsonIgnore]
    private static readonly Dictionary<string, Dictionary<string, string>> DefaultLaunchArgs = [];
    
    [JsonPropertyName("title")]
    public string Title { get; set; } = "empty";
    
    // LaunchId to (argument_key to argument_value)
    [JsonPropertyName("launch_arguments")]
    public Dictionary<string, Dictionary<string, string>> LaunchArguments { get; set; } = DefaultLaunchArgs;
    
    // ModId to version
    [JsonPropertyName("mod_configs")]
    public Dictionary<string, string> ModConfigs { get; set; } = [];
}