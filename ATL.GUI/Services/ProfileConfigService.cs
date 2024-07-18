using ATL.Core.Config.GUI;
using ATL.Core.Libraries;

namespace ATL.GUI.Services;

public class ProfileConfigService
{
    protected Dictionary<string, Dictionary<string, ProfileConfig>> GameProfileConfigs { get; set; } = [];
    public event Action ProfileConfigReloaded = () => { };
    protected LogService LogService { get; set; }

    public ProfileConfigService(LogService? logService = null)
    {
        logService ??= new LogService();
        LogService = logService;
    }

    public ProfileConfig Load(string gameId, string profileId, bool fireEvent = true)
    {
        var optionConfig = ConfigLibrary.LoadProfileConfig(gameId, profileId);
        if (!optionConfig.IsSome(out var profileConfig))
        {
            LogService.Warning($"Failed to load '{profileId}' for '{gameId}'");
            
            if (GameProfileConfigs.TryGetValue(gameId, out var existingProfileConfigs))
            {
                if (existingProfileConfigs.TryGetValue(profileId, out var existingProfileConfig))
                {
                    LogService.Info($"Found existing '{profileId}' for '{gameId}'");
                    return existingProfileConfig;
                }
            }

            LogService.Error($"Failed to find '{profileId}' for '{gameId}'");
            return new ProfileConfig();
        }
        
        lock (GameProfileConfigs)
        {
            if (!GameProfileConfigs.TryGetValue(gameId, out var profileConfigs))
            {
                LogService.Warning($"No profiles found for '{gameId}'");
                profileConfigs = [];
            }

            profileConfigs[profileId] = profileConfig;
            GameProfileConfigs[gameId] = profileConfigs;
        }

        if (fireEvent)
        {
            LogService.Debug("Calling reload event");
            ProfileConfigReloaded.Invoke();
        }
        
        return profileConfig;
    }

    public Dictionary<string, ProfileConfig> LoadGame(string gameId, bool fireEvent = true)
    {
        var configsPath = ConfigLibrary.GetProfileConfigPath(gameId);
        if (!Directory.Exists(configsPath))
        {
            LogService.Info("Creating directory");
            Directory.CreateDirectory(configsPath);
        }
        
        var profileFilePaths = Directory.GetFiles(configsPath, "*.json");
        var profileConfigs = new Dictionary<string, ProfileConfig>();

        if (profileFilePaths.Length == 0)
        {
            LogService.Warning($"No profiles found for '{gameId}'");
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

        if (fireEvent)
        {
            LogService.Debug("Calling reload event");
            ProfileConfigReloaded.Invoke();
        }
        
        return profileConfigs;
    }

    public Task<ProfileConfig> LoadAsync(string gameId, string profileId, bool fireEvent = true)
    {
        var result = Task.FromResult(Load(gameId, profileId, fireEvent));
        return result;
    }

    public void Save(string gameId, string profileId, ProfileConfig profileConfig)
    {
        Task.Run(() =>
        {
            ConfigLibrary.SaveProfileConfig(profileConfig, profileId, gameId);
            Load(gameId, profileId);
        });
    }
    
    public void Update(string gameId, string oldProfileId, ProfileConfig profileConfig)
    {
        try
        {
            Delete(gameId, oldProfileId);
        }
        catch
        {
            LogService.Error($"Failed to update '{oldProfileId}' from '{gameId}'");
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
                LogService.Warning($"'{gameId}' for '{profileId}' not loaded");
            }
        }
        else
        {
            LogService.Warning($"'{gameId}' for '{profileId}' not loaded");
        }
        
        return result;
    }

    public Dictionary<string, ProfileConfig> GetGame(string gameId)
    {
        var result = new Dictionary<string, ProfileConfig>();
        if (GameProfileConfigs.TryGetValue(gameId, out var profileConfigs))
        {
            result = profileConfigs;
        }
        else
        {
            LogService.Warning($"'{gameId} not loaded");
        }
        
        return result;
    }
    
    public Dictionary<string, Dictionary<string, ProfileConfig>> GetAll()
    {
        var result = GameProfileConfigs;
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
            LogService.Error($"Failed to delete '{profileId}' from '{gameId}'");
            return;
        }

        if (GameProfileConfigs.TryGetValue(gameId, out var profileConfigs))
        {
            profileConfigs.Remove(profileId);
        }
    }

    public void RegisterConfigReload(Action action)
    {
        LogService.Debug("Adding to reload event");
        ProfileConfigReloaded += action;
    }

    public void UnregisterConfigReload(Action action)
    {
        LogService.Debug("Removing from reload event");
        ProfileConfigReloaded -= action;
    }
}