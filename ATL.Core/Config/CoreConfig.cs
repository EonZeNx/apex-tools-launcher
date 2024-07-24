using ATL.Core.Libraries;

namespace ATL.Core.Config;

public static class CoreConfig
{
    public static AppConfig AppConfig { get; set; } = new();
    
    public static void LoadAppConfig()
    {
        var optionConfig = ConfigLibrary.LoadAppConfig();
        if (!optionConfig.IsSome(out var config))
        {
            AppConfig = new AppConfig();
            ConsoleLibrary.Log("Failed to load AppConfig, using defaults as fallback", LogType.Error);
            return;
        }

        AppConfig = config;
    }
}
