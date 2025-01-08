using ATL.Core.Config;
using ATL.Core.Libraries;
using ATL.GUI.Services.Development;

namespace ATL.GUI.Services.App;

public class AppConfigService
{
    public static string AppConfigFileName = "atl_config";
    
    protected AppConfig AppConfig { get; set; } = new();
    protected event Action AppConfigReloaded = () => { };
    protected ILogService? LogService { get; set; }
    
    public AppConfigService(ILogService? logService = null)
    {
        LogService = logService;
    }

    public AppConfig Load(bool fireEvent = true)
    {
        var optionConfig = ConfigLibrary.LoadAppConfig();
        if (!optionConfig.IsSome(out var appConfig))
        {
            LogService?.Error("Failed to load");
            return AppConfig;
        }
        
        lock (AppConfig)
        {
            AppConfig = appConfig;
        }

        if (fireEvent)
        {
            LogService?.Debug("Calling reload event");
            AppConfigReloaded.Invoke();
        }
        
        return AppConfig;
    }

    public Task<AppConfig> LoadAsync(bool fireEvent = true)
    {
        var result = Task.FromResult(Load(fireEvent));
        return result;
    }

    public void Save(AppConfig appConfig)
    {
        Task.Run(() =>
        {
            ConfigLibrary.SaveAppConfig(appConfig);
            Load();
        });
    }

    public AppConfig Get()
    {
        var result = AppConfig;
        return result;
    }

    public Task<AppConfig> GetAsync()
    {
        var result = Task.FromResult(AppConfig);
        return result;
    }


    public string BasePath() => AppDomain.CurrentDomain.BaseDirectory;
    public string AppConfigPath() => Path.Join(BasePath(), $"{AppConfigFileName}.json");
    public string GameConfigPath() => Path.Join(BasePath(), AppConfig.GameConfigPath);
    public string ProfileConfigPath(string gameId) => Path.Join(BasePath(), AppConfig.ProfileConfigPath, gameId);
    public string ModConfigPath(string gameId, string modId) => Path.Join(BasePath(), AppConfig.ModsPath, gameId, modId);
    public string ModTargetPath(string modId, string targetVersion) => Path.Join(modId, targetVersion);
    

    public void RegisterConfigReload(Action action)
    {
        LogService?.Debug("Adding to reload event");
        AppConfigReloaded += action;
    }

    public void UnregisterConfigReload(Action action)
    {
        LogService?.Debug("Removing from reload event");
        AppConfigReloaded -= action;
    }
}