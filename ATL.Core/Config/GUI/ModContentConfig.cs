using System.Text.Json.Serialization;

namespace ATL.Core.Config.GUI;

public class ModContentConfig : ICloneable
{
    [JsonPropertyName("title")]
    public string Title { get; set; } = "Title";
    
    [JsonPropertyName("description")]
    public string Description { get; set; } = "";
    
    [JsonPropertyName("target")]
    public string Target { get; set; } = "";

    protected bool Equals(ModContentConfig other)
    {
        return Title == other.Title &&
               Description == other.Description &&
               Target == other.Target;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((ModContentConfig)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Title, Description, Target);
    }

    public object Clone()
    {
        return MemberwiseClone();
    }
}
