using System.Text.Json.Serialization;
using ApexToolsLauncher.Core.Libraries;

namespace ApexToolsLauncher.Core.Config;

public class AppState
{
    [JsonPropertyName("last_page")]
    public string LastPage { get; set; } = "manage";
    
    [JsonPropertyName("last_game_id")]
    public string LastGameId { get; set; } = ConstantsLibrary.InvalidString;
    
    [JsonIgnore]
    public Dictionary<string, string> LastProfileId { get; set; } = [];
    
    // ReSharper disable once UnusedMember.Global
    [JsonPropertyName("last_profile_id")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, string>? JsonLastProfileId
    {
        get => LastProfileId.Count != 0 ? JsonLastProfileId : null;
        set => LastProfileId = value ?? [];
    }
}
