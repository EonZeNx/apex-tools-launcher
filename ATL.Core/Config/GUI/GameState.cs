using System.Text.Json.Serialization;

namespace ATL.Core.Config.GUI;

public class GameState
{
    [JsonPropertyName("profile")]
    public string Profile { get; set; } = "Profile";
}