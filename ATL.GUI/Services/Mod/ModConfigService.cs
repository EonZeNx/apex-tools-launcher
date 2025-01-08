using System.Text.Json;
using ATL.Core.Config.GUI;
using ATL.Core.Libraries;
using ATL.GUI.Services.App;
using ATL.GUI.Services.Development;
using RustyOptions;

namespace ATL.GUI.Services.Mod;

public class ModConfigService : IModConfigService
{
    protected Dictionary<string, Dictionary<string, ModConfig>> ModConfigs { get; set; } = [];
    protected event Action ConfigReloaded = () => { };
    protected ILogService? LogService { get; set; }
    protected AppConfigService AppConfigService { get; set; }

    public ModConfigService(ILogService? logService = null, AppConfigService? appConfigService = null)
    {
        LogService = logService;
        
        appConfigService ??= new AppConfigService();
        AppConfigService = appConfigService;
        
        LoadAll();
    }

    
    public void Load(string gameId, string modId)
    {
        var optionConfig = Option<ModConfig>.None;
        
        var configPath = Path.Join(AppConfigService.ModConfigPath(gameId, modId), $"{ConstantsLibrary.ModConfigFileName}.json");
        if (File.Exists(configPath))
        {
            try
            {
                var jsonString = File.ReadAllText(configPath);
                var config = JsonSerializer.Deserialize<ModConfig>(jsonString);
                if (config is not null)
                {
                    optionConfig = new Option<ModConfig>(config);
                }
            }
            catch { /* ignored */ }
        }
        
        if (!optionConfig.IsSome(out var modConfig))
        {
            LogService?.Warning($"Failed to load '{modId}' for '{gameId}'");
            
            if (ModConfigs.TryGetValue(gameId, out var existingModConfigs))
            {
                if (existingModConfigs.ContainsKey(modId))
                {
                    LogService?.Info($"Found existing '{modId}' for '{gameId}'");
                    return;
                }
            }

            LogService?.Error($"Failed to find '{modId}' for '{gameId}'");
            return;
        }
        
        lock (ModConfigs)
        {
            if (!ModConfigs.TryGetValue(gameId, out var modConfigs))
            {
                LogService?.Warning($"No mods found for '{gameId}'");
                modConfigs = [];
            }

            modConfigs[modId] = modConfig;
            ModConfigs[gameId] = modConfigs;
        }

        LogService?.Debug("Calling reload event");
        ConfigReloaded.Invoke();
    }

    public Task LoadAsync(string gameId, string modId)
    {
        var result = new Task(() => Load(gameId, modId));
        return result;
    }
    
    
    public void LoadAllFromGame(string gameId)
    {
        var configsPath = ConfigLibrary.GetModConfigPath(gameId);
        if (!Directory.Exists(configsPath))
        {
            LogService?.Info("Creating directory");
            Directory.CreateDirectory(configsPath);
        }
        
        var modFilePaths = Directory.GetFiles(configsPath, "*.json", SearchOption.AllDirectories);
        var modConfigs = new Dictionary<string, ModConfig>();

        if (modFilePaths.Length == 0)
        {
            LogService?.Warning($"No mods found for '{gameId}'");
        }
        
        foreach (var modFilePath in modFilePaths)
        {
            var modDirectory = Directory.GetParent(modFilePath);
            var modId = modDirectory?.Name ?? "ModId";

            var optionConfig = ConfigLibrary.LoadModConfig(modFilePath);
            if (optionConfig.IsSome(out var config))
            {
                modConfigs.Add(modId, config);
            }
        }
        
        lock (ModConfigs)
        {
            ModConfigs[gameId] = modConfigs;
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


    public ModConfig Get(string gameId, string modId)
    {
        var result = new ModConfig();
        
        if (ModConfigs.TryGetValue(gameId, out var modConfigs))
        {
            if (modConfigs.TryGetValue(modId, out var modConfig))
            {
                result = modConfig;
            }
            else
            {
                LogService?.Warning($"'{modId}' for '{gameId}' not loaded");
            }
        }
        else
        {
            LogService?.Warning($"'{modId}' for '{gameId}' not loaded");
        }
        
        return result;
    }
    
    public Task<ModConfig> GetAsync(string gameId, string modId)
    {
        var result = Task.FromResult(Get(gameId, modId));
        return result;
    }
    
    
    public Dictionary<string, ModConfig> GetAllFromGame(string gameId)
    {
        var result = new Dictionary<string, ModConfig>();
        
        if (ModConfigs.TryGetValue(gameId, out var modConfigs))
        {
            result = modConfigs;
        }
        else
        {
            LogService?.Warning($"'{gameId} not loaded");
        }
        
        return result;
    }

    public Task<Dictionary<string, ModConfig>> GetAllFromGameAsync(string gameId)
    {
        var result = Task.FromResult(GetAllFromGame(gameId));
        return result;
    }


    public bool Contains(string gameId, string modId)
    {
        if (ModConfigs.TryGetValue(gameId, out var modConfigs))
        {
            return modConfigs.ContainsKey(modId);
        }

        return false;
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