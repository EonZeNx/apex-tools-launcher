using ATL.Core.Libraries;

namespace ATL.Core.Config;

public static class CoreAppConfig
{
    private static AppConfig AppConfig { get; set; } = new();
    public static AppConfig Get() => AppConfig;
    
    public static void LoadAppConfig()
    {
        var optionConfig = ConfigLibrary.LoadAppConfig();
        if (optionConfig.IsNone)
        {
            AppConfig = new AppConfig();
            ConsoleLibrary.Log("Failed to load AppConfig, using defaults as fallback", LogType.Error);
            return;
        }

        AppConfig = optionConfig.Unwrap();
    }
}
