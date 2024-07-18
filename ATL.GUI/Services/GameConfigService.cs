using ATL.Core.Config.GUI;
using ATL.Core.Libraries;

namespace ATL.GUI.Services;

public class GameConfigService
{
    protected Dictionary<string, GameConfig> GameConfigs { get; set; } = [];
    public event Action GameConfigReloaded = () => { };
    protected LogService LogService { get; set; }

    public GameConfigService(LogService? logService = null)
    {
        logService ??= new LogService();
        LogService = logService;
    }

    public GameConfig Load(string gameId, bool fireEvent = true)
    {
        var optionConfig = ConfigLibrary.LoadGameConfig(gameId);
        if (!optionConfig.IsSome(out var gameConfig))
        {
            LogService.Warning($"Failed to load '{gameId}'");
            
            if (GameConfigs.TryGetValue(gameId, out var existingGameConfig))
            {
                LogService.Info($"Found existing GameConfig for '{gameId}'");
                return existingGameConfig;
            }
            
            LogService.Error($"Failed to find {gameId}");
            return new GameConfig();
        }
        
        lock (GameConfigs)
        {
            GameConfigs[gameId] = gameConfig;
        }

        if (fireEvent)
        {
            LogService.Debug("Calling reload event");
            GameConfigReloaded.Invoke();
        }
        
        return gameConfig;
    }
    
    public Dictionary<string, GameConfig> LoadAll(bool fireEvent = true)
    {
        var gameConfigs = ConfigLibrary.LoadGameConfigs();
        
        lock (GameConfigs)
        {
            GameConfigs = gameConfigs;
        }

        if (fireEvent)
        {
            LogService.Debug("Calling reload event");
            GameConfigReloaded.Invoke();
        }
        
        return gameConfigs;
    }

    public Task<GameConfig> LoadAsync(string gameId, bool fireEvent = true)
    {
        var result = Task.FromResult(Load(gameId, fireEvent));
        return result;
    }

    public void Save(string gameId, GameConfig gameConfig)
    {
        Task.Run(() =>
        {
            ConfigLibrary.SaveGameConfig(gameConfig, gameId);
            Load(gameId);
        });
    }

    public GameConfig Get(string gameId)
    {
        var result = new GameConfig();
        if (GameConfigs.TryGetValue(gameId, out var gameConfig))
        {
            result = gameConfig;
        }
        else
        {
            LogService.Warning($"'{gameId}' not loaded");
        }
        
        return result;
    }
    
    public Dictionary<string, GameConfig> GetAll()
    {
        var result = GameConfigs;
        return result;
    }

    public string GetSelectedProfile(string gameId)
    {
        var result = ConstantsLibrary.InvalidString;
        if (GameConfigs.TryGetValue(gameId, out var gameConfig))
        {
            result = gameConfig.SelectedProfile;
        }

        return result;
    }

    public void RegisterConfigReload(Action action)
    {
        LogService.Debug("Adding to reload event");
        GameConfigReloaded += action;
    }

    public void UnregisterConfigReload(Action action)
    {
        LogService.Debug("Removing from reload event");
        GameConfigReloaded -= action;
    }
}