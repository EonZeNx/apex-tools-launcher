using System.Text.Json.Serialization;

namespace ATL.Core.Config.CLI;

public class CliConfig
{
    [JsonPropertyName("auto_close")]
    public bool AutoClose { get; set; } = true;
    
    [JsonPropertyName("lookup_hash")]
    public bool LookupHash { get; set; } = true;
}
