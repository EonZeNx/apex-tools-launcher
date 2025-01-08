namespace ATL.Core.Libraries;

public static class ConstantsLibrary
{
    public static string AppFullTitle => "Apex Launcher & Tools";
    public static string AppTitle => "ALT";
    public static string AppVersion => "v0.13.0";
    public static int[] AppWindowSize => [1280, 900];
    
    public static string AppConfigFileName = "alt_config";
    public static string AppStateFileName = "alt_state";
    public static string ModConfigFileName = "mod_config";
    
    public static string InvalidString => "?";

    public static bool IsStringInvalid(string value)
    {
        var result = string.Equals(value, InvalidString);
        return result;
    }

    public static string CreateId(string value)
    {
        var result = value.ToLowerInvariant().Replace(" ", "_");
        result = string.Join("_", result.Split(Path.GetInvalidFileNameChars()));
        
        return result;
    }
}
