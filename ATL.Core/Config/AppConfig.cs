using System.Text.Json.Serialization;
using ATL.Core.Config.CLI;

namespace ATL.Core.Config;

public class AppConfig
{
    [JsonPropertyName("game_config_path")]
    public string GameConfigPath { get; set; } = "game_config";
    
    [JsonPropertyName("profile_config_path")]
    public string ProfileConfigPath { get; set; } = "profile_config";
    
    [JsonPropertyName("mods_path")]
    public string ModsPath { get; set; } = "mods";
    
    [JsonPropertyName("vfs_fs_path")]
    public string VfsFsPath { get; set; } = "vfs_fs";
    
    [JsonPropertyName("steam_path")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string SteamPath { get; set; } = @"C:\Program Files (x86)\Steam";

    [JsonPropertyName("cli")]
    public CliConfig Cli { get; set; } = new();
}
