using ATL.Core.Config.GUI;
using ATL.Core.Libraries;
using ATL.GUI.Services.Development;

namespace ATL.GUI.Services.Game;

public class GameConfigService : IGameConfigService
{
    protected Dictionary<string, GameConfig> GameConfigs { get; set; } = [];
    protected event Action ConfigReloaded = () => { };
    protected ILogService? LogService { get; set; }

    public GameConfigService(ILogService? logService = null)
    {
        LogService = logService;
        
        LoadAll();
    }

    public void Load(string gameId)
    {
        var optionConfig = ConfigLibrary.LoadGameConfig(gameId);
        if (!optionConfig.IsSome(out var gameConfig))
        {
            LogService?.Warning($"Failed to load '{gameId}'");
            
            if (GameConfigs.ContainsKey(gameId))
            {
                LogService?.Info($"Found existing GameConfig for '{gameId}'");
                return;
            }
            
            LogService?.Error($"Failed to find {gameId}");
            return;
        }
        
        lock (GameConfigs)
        {
            GameConfigs[gameId] = gameConfig;
        }

        LogService?.Debug("Calling reload event");
        ConfigReloaded.Invoke();
    }
    
    public Task LoadAsync(string gameId)
    {
        var result = new Task(() => Load(gameId));
        return result;
    }
    
    public void LoadAll()
    {
        var gameConfigs = ConfigLibrary.LoadGameConfigs();
        
        lock (GameConfigs)
        {
            GameConfigs = gameConfigs;
        }

        LogService?.Debug("Calling reload event");
        ConfigReloaded.Invoke();
    }
    
    public Task LoadAllAsync()
    {
        var result = new Task(LoadAll);
        return result;
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
            LogService?.Warning($"'{gameId}' not loaded");
        }
        
        return result;
    }
    
    public Task<GameConfig> GetAsync(string gameId)
    {
        var result = Task.FromResult(Get(gameId));
        return result;
    }


    public GameConfig GetOrLoad(string gameId)
    {
        if (GameConfigs.TryGetValue(gameId, out var gameConfig))
        {
            return gameConfig;
        }
        
        Load(gameId);
        return Get(gameId);
    }

    public Task<GameConfig> GetOrLoadAsync(string gameId)
    {
        var result = Task.FromResult(GetOrLoad(gameId));
        return result;
    }


    public Dictionary<string, GameConfig> GetAll()
    {
        var result = GameConfigs;
        return result;
    }
    
    public Task<Dictionary<string, GameConfig>> GetAllAsync()
    {
        var result = Task.FromResult(GetAll());
        return result;
    }

    public void RegisterConfigReload(Action action)
    {
        LogService?.Debug("Adding to reload event");
        ConfigReloaded += action;
    }

    public void UnregisterConfigReload(Action action)
    {
        LogService?.Debug("Removing from reload event");
        ConfigReloaded -= action;
    }
}