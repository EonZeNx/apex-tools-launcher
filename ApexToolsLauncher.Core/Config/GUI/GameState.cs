using System.Text.Json.Serialization;

namespace ApexToolsLauncher.Core.Config.GUI;

public class GameState
{
    [JsonPropertyName("profile")]
    public string Profile { get; set; } = "Profile";
}