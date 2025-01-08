using ATL.Core.Config.GUI;
using ATL.Core.Libraries;
using ATL.GUI.Services.Development;

namespace ATL.GUI.Services.Mod;

public class ProfileConfigService : IProfileConfigService
{
    protected Dictionary<string, Dictionary<string, ProfileConfig>> GameProfileConfigs { get; set; } = [];
    protected event Action ConfigReloaded = () => { };
    protected ILogService? LogService { get; set; }

    public ProfileConfigService(ILogService? logService = null)
    {
        LogService = logService;

        LoadAll();
    }

    
    public void Load(string gameId, string profileId)
    {
        var optionConfig = ConfigLibrary.LoadProfileConfig(gameId, profileId);
        if (!optionConfig.IsSome(out var profileConfig))
        {
            LogService?.Warning($"Failed to load '{profileId}' for '{gameId}'");
            
            if (GameProfileConfigs.TryGetValue(gameId, out var existingProfileConfigs))
            {
                if (existingProfileConfigs.ContainsKey(profileId))
                {
                    LogService?.Info($"Found existing '{profileId}' for '{gameId}'");
                    return;
                }
            }

            LogService?.Error($"Failed to find '{profileId}' for '{gameId}'");
            return;
        }
        
        lock (GameProfileConfigs)
        {
            if (!GameProfileConfigs.TryGetValue(gameId, out var profileConfigs))
            {
                LogService?.Warning($"No profiles found for '{gameId}'");
                profileConfigs = [];
            }

            profileConfigs[profileId] = profileConfig;
            GameProfileConfigs[gameId] = profileConfigs;
        }

        LogService?.Debug("Calling reload event");
        ConfigReloaded.Invoke();
    }

    public Task LoadAsync(string gameId, string profileId)
    {
        var result = new Task(() => Load(gameId, profileId));
        return result;
    }


    public void LoadAllFromGame(string gameId)
    {
        var configsPath = ConfigLibrary.GetProfileConfigPath(gameId);
        if (!Directory.Exists(configsPath))
        {
            LogService?.Info("Creating directory");
            Directory.CreateDirectory(configsPath);
        }
        
        var profileFilePaths = Directory.GetFiles(configsPath, "*.json");
        var profileConfigs = new Dictionary<string, ProfileConfig>();

        if (profileFilePaths.Length == 0)
        {
            LogService?.Warning($"No profiles found for '{gameId}'");
        }
        
        foreach (var profileFilePath in profileFilePaths)
        {
            var profileId = Path.GetFileNameWithoutExtension(profileFilePath);

            var optionConfig = ConfigLibrary.LoadProfileConfig(gameId, profileId);
            if (optionConfig.IsSome(out var config))
            {
                profileConfigs.Add(profileId, config);
            }
        }
        
        lock (GameProfileConfigs)
        {
            GameProfileConfigs[gameId] = profileConfigs;
        }

        LogService?.Debug("Calling reload event");
        ConfigReloaded.Invoke();
    }

    public Task LoadAllFromGameAsync(string gameId)
    {
        var result = new Task(() => LoadAllFromGame(gameId));
        return result;
    }


    public void LoadAll()
    {
        var gameIds = ConfigLibrary.GetAllGameIds();

        foreach (var gameId in gameIds)
        {
            LoadAllFromGame(gameId);
        }
    }

    public Task LoadAllAsync()
    {
        var result = new Task(LoadAll);
        return result;
    }


    public ProfileConfig Get(string gameId, string profileId)
    {
        var result = new ProfileConfig();
        if (GameProfileConfigs.TryGetValue(gameId, out var profileConfigs))
        {
            if (profileConfigs.TryGetValue(profileId, out var profileConfig))
            {
                result = profileConfig;
            }
            else
            {
                LogService?.Warning($"'{gameId}' for '{profileId}' not loaded");
            }
        }
        else
        {
            LogService?.Warning($"'{gameId}' for '{profileId}' not loaded");
        }
        
        return result;
    }
    
    public Task<ProfileConfig> GetAsync(string gameId, string profileId)
    {
        var result = Task.FromResult(Get(gameId, profileId));
        return result;
    }
    

    public Dictionary<string, ProfileConfig> GetAllFromGame(string gameId)
    {
        var result = new Dictionary<string, ProfileConfig>();
        if (GameProfileConfigs.TryGetValue(gameId, out var profileConfigs))
        {
            result = profileConfigs;
        }
        else
        {
            LogService?.Warning($"'{gameId} not loaded");
        }
        
        return result;
    }
    
    public Task<Dictionary<string, ProfileConfig>> GetAllFromGameAsync(string gameId)
    {
        var result = Task.FromResult(GetAllFromGame(gameId));
        return result;
    }
    
    
    public void Save(string gameId, string profileId, ProfileConfig profileConfig)
    {
        ConfigLibrary.SaveProfileConfig(profileConfig, profileId, gameId);
        Load(gameId, profileId);
    }
    
    public Task SaveAsync(string gameId, string profileId, ProfileConfig profileConfig)
    {
        var result = new Task(() => Save(gameId, profileId, profileConfig));
        return result;
    }
    
    
    public void Delete(string gameId, string profileId)
    {
        try
        {
            ConfigLibrary.DeleteProfile(gameId, profileId);
        }
        catch
        {
            LogService?.Error($"Failed to delete '{profileId}' from '{gameId}'");
            return;
        }

        if (GameProfileConfigs.TryGetValue(gameId, out var profileConfigs))
        {
            profileConfigs.Remove(profileId);
        }
    }
    
    public Task DeleteAsync(string gameId, string profileId)
    {
        var result = new Task(() => Delete(gameId, profileId));
        return result;
    }
    
    
    public void RegisterOnReload(Action action)
    {
        LogService?.Debug("Adding to reload event");
        ConfigReloaded += action;
    }

    public void UnregisterOnReload(Action action)
    {
        LogService?.Debug("Removing from reload event");
        ConfigReloaded -= action;
    }
    
    
    public void Update(string gameId, string oldProfileId, ProfileConfig profileConfig)
    {
        try
        {
            Delete(gameId, oldProfileId);
        }
        catch
        {
            LogService?.Error($"Failed to update '{oldProfileId}' from '{gameId}'");
            return;
        }

        var profileId = ConstantsLibrary.CreateId(profileConfig.Title);
        Save(gameId, profileId, profileConfig);
    }

    public bool ProfileExists(string gameId, string profileId)
    {
        var result = false;
        if (GameProfileConfigs.TryGetValue(gameId, out var profileConfigs))
        {
            result = profileConfigs.ContainsKey(profileId);
        }

        return result;
    }
}