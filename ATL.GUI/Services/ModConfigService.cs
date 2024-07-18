using System.Text.Json;
using ATL.Core.Config.GUI;
using ATL.Core.Libraries;
using RustyOptions;

namespace ATL.GUI.Services;

public class ModConfigService
{
    protected Dictionary<string, Dictionary<string, ModConfig>> GameModConfigs { get; set; } = [];
    public event Action ModConfigReloaded = () => { };
    protected LogService LogService { get; set; }
    protected AppConfigService AppConfigService { get; set; }

    public ModConfigService(LogService? logService = null, AppConfigService? appConfigService = null)
    {
        logService ??= new LogService();
        LogService = logService;
        
        appConfigService ??= new AppConfigService();
        AppConfigService = appConfigService;
    }

    public ModConfig Load(string gameId, string modId, bool fireEvent = true)
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
            LogService.Warning($"Failed to load '{modId}' for '{gameId}'");
            
            if (GameModConfigs.TryGetValue(gameId, out var existingModConfigs))
            {
                if (existingModConfigs.TryGetValue(modId, out var existingModConfig))
                {
                    LogService.Info($"Found existing '{modId}' for '{gameId}'");
                    return existingModConfig;
                }
            }

            LogService.Error($"Failed to find '{modId}' for '{gameId}'");
            return new ModConfig();
        }
        
        lock (GameModConfigs)
        {
            if (!GameModConfigs.TryGetValue(gameId, out var modConfigs))
            {
                LogService.Warning($"No mods found for '{gameId}'");
                modConfigs = [];
            }

            modConfigs[modId] = modConfig;
            GameModConfigs[gameId] = modConfigs;
        }

        if (fireEvent)
        {
            LogService.Debug("Calling reload event");
            ModConfigReloaded.Invoke();
        }
        
        return modConfig;
    }

    public Task<ModConfig> LoadAsync(string gameId, string modId, bool fireEvent = true)
    {
        var result = Task.FromResult(Load(gameId, modId, fireEvent));
        return result;
    }
    
    public Dictionary<string, ModConfig> LoadGame(string gameId, bool fireEvent = true)
    {
        var configsPath = ConfigLibrary.GetModConfigPath(gameId);
        if (!Directory.Exists(configsPath))
        {
            LogService.Info("Creating directory");
            Directory.CreateDirectory(configsPath);
        }
        
        var modFilePaths = Directory.GetFiles(configsPath, "*.json", SearchOption.AllDirectories);
        var modConfigs = new Dictionary<string, ModConfig>();

        if (modFilePaths.Length == 0)
        {
            LogService.Warning($"No mods found for '{gameId}'");
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
        
        lock (GameModConfigs)
        {
            GameModConfigs[gameId] = modConfigs;
        }

        if (fireEvent)
        {
            LogService.Debug("Calling reload event");
            ModConfigReloaded.Invoke();
        }
        
        return modConfigs;
    }
    
    public void Save(string gameId, string modId, ModConfig modConfig)
    {
        var configPath = Path.Join(AppConfigService.ModConfigPath(gameId, modId), $"{ConstantsLibrary.ModConfigFileName}.json");
        if (!File.Exists(configPath))
        {
            Directory.CreateDirectory(AppConfigService.ModConfigPath(gameId, modId));
        }

        var jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        var jsonString = JsonSerializer.Serialize(modConfig, jsonOptions);
        File.WriteAllText(configPath, jsonString);
        
        Load(gameId, modId);
    }
    
    public void Delete(string gameId, string modId)
    {
        try
        {
            var configPath = Path.Join(AppConfigService.ModConfigPath(gameId, modId), $"{ConstantsLibrary.ModConfigFileName}.json");
            if (File.Exists(configPath))
            {
                File.Delete(configPath);
            }
        }
        catch
        {
            LogService.Error($"Failed to delete '{modId}' from '{gameId}'");
            return;
        }

        if (GameModConfigs.TryGetValue(gameId, out var modConfigs))
        {
            modConfigs.Remove(modId);
        }
    }
    
    public ModConfig Get(string gameId, string modId)
    {
        var result = new ModConfig();
        
        if (GameModConfigs.TryGetValue(gameId, out var modConfigs))
        {
            if (modConfigs.TryGetValue(modId, out var modConfig))
            {
                result = modConfig;
            }
            else
            {
                LogService.Warning($"'{modId}' for '{gameId}' not loaded");
            }
        }
        else
        {
            LogService.Warning($"'{modId}' for '{gameId}' not loaded");
        }
        
        return result;
    }
    
    public Dictionary<string, ModConfig> GetGame(string gameId)
    {
        var result = new Dictionary<string, ModConfig>();
        
        if (GameModConfigs.TryGetValue(gameId, out var modConfigs))
        {
             result = modConfigs;
        }
        else
        {
            LogService.Warning($"'{gameId} not loaded");
        }
        
        return result;
    }

    public bool Exists(string gameId, string modId)
    {
        var result = true;
        if (GameModConfigs.TryGetValue(gameId, out var modConfigs))
        {
            result &= modConfigs.ContainsKey(modId);
        }

        if (result)
        {
            var configPath = Path.Join(AppConfigService.ModConfigPath(gameId, modId), $"{ConstantsLibrary.ModConfigFileName}.json");
            result = File.Exists(configPath);
        }

        return result;
    }
    
    public bool ContentExists(string gameId, string modId, string contentId)
    {
        var result = Exists(gameId, modId);
        if (result)
        {
            if (GameModConfigs.TryGetValue(gameId, out var modConfigs))
            {
                if (modConfigs.TryGetValue(modId, out var modConfig))
                {
                    result &= modConfig.Versions.ContainsKey(contentId);
                }
            }
        }

        return result;
    }
    

    public void RegisterConfigReload(Action action)
    {
        LogService.Debug("Adding to reload event");
        ModConfigReloaded += action;
    }

    public void UnregisterConfigReload(Action action)
    {
        LogService.Debug("Removing from reload event");
        ModConfigReloaded -= action;
    }
}