using System.Text.Json.Serialization;

namespace ATL.Core.Config.CLI;

public class CliConfig
{
    [JsonPropertyName("auto_close")]
    public bool AutoClose { get; set; } = true;
    
    [JsonPropertyName("log_path")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string LogPath { get; set; } = "logs";
    
    [JsonPropertyName("database_path")]
    public string DatabasePath { get; set; } = Path.Join("databases", "atl.core.db");
    
    [JsonPropertyName("lookup_hash")]
    public bool LookupHash { get; set; } = true;
}
