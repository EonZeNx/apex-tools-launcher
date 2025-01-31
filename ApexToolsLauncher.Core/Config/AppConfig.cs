using System.Text.Json.Serialization;
using ApexToolsLauncher.Core.Config.CLI;

namespace ApexToolsLauncher.Core.Config;

public class AppConfig
{ // todo: update relative paths to absolute
    [JsonPropertyName("log_path")]
    public string LogPath { get; set; } = Path.Join(AppDomain.CurrentDomain.BaseDirectory, "logs");
    
    [JsonPropertyName("game_config_path")]
    public string GameConfigPath { get; set; } = "games";
    
    [JsonPropertyName("databases_directory")]
    public string DatabasesDirectory { get; set; } = Path.Join(AppDomain.CurrentDomain.BaseDirectory, "databases");
    
    [JsonPropertyName("profile_config_path")]
    public string ProfileConfigPath { get; set; } = "profiles";
    
    [JsonPropertyName("mods_path")]
    public string ModsPath { get; set; } = "mods";
    
    [JsonPropertyName("vfs_fs_path")]
    public string VfsFsPath { get; set; } = "vfs_fs";
    
    [JsonPropertyName("steam_path")]
    public string SteamPath { get; set; } = @"C:\Program Files (x86)\Steam";
    
    [JsonPropertyName("preload_hash")]
    public bool PreloadHashes { get; set; } = true;

    [JsonPropertyName("cli")]
    public CliConfig Cli { get; set; } = new();
}
