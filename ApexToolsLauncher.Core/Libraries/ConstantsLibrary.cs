namespace ApexToolsLauncher.Core.Libraries;

public static class ConstantsLibrary
{
    public static string AppFullTitle => "Apex Tools & Launcher";
    public static string AppTitle => "ATL";
    public static string AppVersion => "v0.13.1";
    public static int[] AppWindowSize => [1280, 900];
    
    public static string AppConfigFileName = "atl_config";
    public static string AppStateFileName = "atl_state";
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
