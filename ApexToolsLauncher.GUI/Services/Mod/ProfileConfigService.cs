using ApexToolsLauncher.Core.Config.GUI;
using ApexToolsLauncher.Core.Libraries;
using ApexToolsLauncher.GUI.Services.Development;

namespace ApexToolsLauncher.GUI.Services.Mod;

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
        if (ConstantsLibrary.IsStringInvalid(gameId) || ConstantsLibrary.IsStringInvalid(profileId))
        {
            LogService?.Debug($"'{gameId}' or '{profileId}' is invalid");
            return result;
        }
        
        if (GameProfileConfigs.TryGetValue(gameId, out var profileConfigs))
        {
            if (profileConfigs.TryGetValue(profileId, out var profileConfig))
            {
                result = profileConfig;
            }
            else
            {
                LogService?.Warning($"'{profileId}' for '{gameId}' not loaded");
            }
        }
        else
        {
            LogService?.Warning($"'{profileId}' for '{gameId}' not loaded");
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
        var result = Task.Run(() => Save(gameId, profileId, profileConfig));
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

        LoadAllFromGame(gameId);
    }
    
    public Task DeleteAsync(string gameId, string profileId)
    {
        var result = new Task(() => Delete(gameId, profileId));
        return result;
    }

    
    public bool Create(string gameId, string profileId, ProfileConfig? config = null)
    {
        if (IdExists(gameId, profileId))
        {
            return false;
        }

        Save(gameId, profileId, config ?? new ProfileConfig());

        return true;
    }

    public Task<bool> CreateAsync(string gameId, string profileId, ProfileConfig? config = null)
    {
        return Task.FromResult(Create(gameId, profileId, config));
    }

    public string ToId(string value)
    {
        var result = value.ToLowerInvariant().Replace(" ", "_");
        result = string.Join("_", result.Split(Path.GetInvalidFileNameChars()));
        
        return result;
    }
    
    public bool IdExists(string gameId, string profileId)
    {
        var result = false;
        if (GameProfileConfigs.TryGetValue(gameId, out var profileConfigs))
        {
            result = profileConfigs.ContainsKey(profileId);
        }

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
}