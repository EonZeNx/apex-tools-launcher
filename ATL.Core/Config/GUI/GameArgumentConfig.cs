using System.Text.Json.Serialization;

namespace ATL.Core.Config.GUI;

public enum ArgumentType
{
    [JsonPropertyName("string")]
    String,
    [JsonPropertyName("select")]
    Select
}

public class ArgumentConfig
{
    [JsonPropertyName("title")]
    public string Title { get; set; } = "Title";
    
    [JsonPropertyName("value")]
    public string Value { get; set; } = "";
}

public class GameArgumentConfig : ArgumentConfig
{
    [JsonPropertyName("type")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ArgumentType Type { get; set; } = ArgumentType.String;

    
    [JsonIgnore]
    public List<string> Options { get; set; } = [];
    
    // ReSharper disable once UnusedMember.Global
    [JsonPropertyName("options")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? JsonOptions
    {
        get => Options.Count != 0 ? Options : null;
        set => Options = value ?? [];
    }
}