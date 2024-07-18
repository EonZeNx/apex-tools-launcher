using System.Text.Json.Serialization;

namespace ATL.Core.Config;

public class AppState
{
    [JsonPropertyName("last_page")]
    public string LastPage { get; set; } = "home";
}
