using System.Text.Json;
using ATL.Core.Config;
using ATL.Core.Config.GUI;
using RustyOptions;

namespace ATL.Core.Libraries;

public static class ConfigLibrary
{
    public static AppConfig AppConfig = new();
    
    public static string GetBasePath() => AppDomain.CurrentDomain.BaseDirectory;
    public static string GetAppConfigPath() => Path.Join(GetBasePath(), $"{ConstantsLibrary.AppConfigFileName}.json");
    public static string GetGameConfigPath() => Path.Join(GetBasePath(), AppConfig.GameConfigPath);
    public static string GetProfileConfigPath(string gameId) => Path.Join(GetBasePath(), AppConfig.ProfileConfigPath, gameId);
    public static string GetModConfigPath(string gameId) => Path.Join(GetBasePath(), AppConfig.ModsPath, gameId);
    public static string GetModTargetPath(string modId, string targetVersion) => Path.Join(modId, targetVersion);
    
    // TODO: Move IO to relevant services
    public static Option<AppConfig> LoadAppConfig()
    {
        var configPath = GetAppConfigPath();
        if (!File.Exists(configPath))
        {
            Directory.CreateDirectory(GetBasePath());

            var defaultAppConfig = new AppConfig();
            var jsonSerializeOptions = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            var defaultAppConfigJson = JsonSerializer.Serialize(defaultAppConfig, jsonSerializeOptions);
            
            File.WriteAllText(configPath, defaultAppConfigJson);
        }
        
        var optionConfig = LoadConfig<AppConfig>(configPath);
        return optionConfig;
    }

    public static void SaveAppConfig(AppConfig config)
    {
        var configPath = GetAppConfigPath();
        if (!File.Exists(configPath))
        {
            Directory.CreateDirectory(GetBasePath());
        }

        var jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        var jsonString = JsonSerializer.Serialize(config, jsonOptions);
        File.WriteAllText(configPath, jsonString);
    }
    
    public static void SaveGameConfig(GameConfig config, string gameId)
    {
        var configPath = Path.Join(GetGameConfigPath(), $"{gameId}.json");
        if (!File.Exists(configPath))
        {
            Directory.CreateDirectory(GetGameConfigPath());
        }

        var jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        var jsonString = JsonSerializer.Serialize(config, jsonOptions);
        File.WriteAllText(configPath, jsonString);
    }
    
    public static Option<GameConfig> LoadGameConfig(string gameId)
    {
        var configPath = Path.Join(GetGameConfigPath(), $"{gameId}.json");
        if (!File.Exists(configPath))
        {
            SaveGameConfig(new GameConfig(), gameId);
        }

        var optionConfig = LoadConfig<GameConfig>(configPath);
        return optionConfig;
    }
    
    public static Dictionary<string, GameConfig> LoadGameConfigs()
    {
        var result = new Dictionary<string, GameConfig>();

        var configsPath = GetGameConfigPath();
        if (!Directory.Exists(configsPath))
        {
            Directory.CreateDirectory(configsPath);
        }
        
        var jsonFilePaths = Directory.GetFiles(configsPath, "*.json");
        
        foreach (var jsonFilePath in jsonFilePaths)
        {
            var jsonFileName = Path.GetFileNameWithoutExtension(jsonFilePath);

            var optionConfig = LoadGameConfig(jsonFileName);
            if (optionConfig.IsSome(out var config))
            {
                result.Add(jsonFileName, config);
            }
        }

        return result;
    }

    public static void SaveProfileConfig(ProfileConfig config, string profileId,string gameId)
    {
        var configPath = Path.Join(GetProfileConfigPath(gameId), $"{profileId}.json");
        if (!File.Exists(configPath))
        {
            Directory.CreateDirectory(GetProfileConfigPath(gameId));
        }

        var jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        var jsonString = JsonSerializer.Serialize(config, jsonOptions);
        File.WriteAllText(configPath, jsonString);
    }
    
    public static Option<ProfileConfig> LoadProfileConfig(string gameId, string profileId)
    {
        var configPath = Path.Join(GetProfileConfigPath(gameId), $"{profileId}.json");
        if (!File.Exists(configPath))
        {
            return Option<ProfileConfig>.None;
        }
        
        try
        {
            var jsonString = File.ReadAllText(configPath);
            var config = JsonSerializer.Deserialize<ProfileConfig>(jsonString);
            if (config is not null)
            {
                return new Option<ProfileConfig>(config);
            }
        }
        catch { /* ignored */ }

        return Option<ProfileConfig>.None;
    }
    
    public static string GetSelectedProfileId(string gameId)
    {
        var optionConfig = LoadGameConfig(gameId);
        return optionConfig.IsSome(out var gameConfig)
            ? gameConfig.SelectedProfile
            : ConstantsLibrary.InvalidString;
    }

    public static void DeleteProfile(string gameId, string profileId)
    {
        var configPath = Path.Join(GetProfileConfigPath(gameId), $"{profileId}.json");
        if (File.Exists(configPath))
        {
            File.Delete(configPath);
        }
    }
    
    public static Option<ModConfig> LoadModConfig(string jsonFilePath)
    {
        if (!File.Exists(jsonFilePath))
        {
            return Option<ModConfig>.None;
        }
        
        try
        {
            var jsonString = File.ReadAllText(jsonFilePath);
            var config = JsonSerializer.Deserialize<ModConfig>(jsonString);
            if (config is not null)
            {
                return new Option<ModConfig>(config);
            }
        }
        catch { /* ignored */ }

        return Option<ModConfig>.None;
    }

    public static IEnumerable<string> GetLaunchArguments(GameConfig gameConfig, ProfileConfig profileConfig)
    {
        var launchArguments = new List<string>();
        var gameLaunchArguments = gameConfig.LaunchOptions;

        foreach (var (launchId, fullArgMap) in profileConfig.LaunchArguments)
        {
            if (!gameLaunchArguments.TryGetValue(launchId, out var profileLaunchConfig))
            {
                continue;
            }

            var fullArguments = profileLaunchConfig.Arguments;
            var arguments = fullArguments;
            foreach (var (argumentKey, argumentConfig) in profileLaunchConfig.ArgumentConfig)
            {
                var replaceValue = argumentConfig.Value;
                if (fullArgMap.TryGetValue(argumentKey, out var profileArgumentValue))
                {
                    replaceValue = profileArgumentValue;
                }

                arguments = arguments.Replace($"{{{argumentKey}}}", replaceValue);
            }
            
            launchArguments.Add(arguments);
        }

        return launchArguments;
    }

    public static string CreateLaunchArgument(LaunchOptionConfig launchConfig, ProfileConfig profileConfig, string launchId)
    {
        var fullArgMap = new Dictionary<string, string>();
        if (profileConfig.LaunchArguments.TryGetValue(launchId, out var tempArgMap))
        {
            fullArgMap = tempArgMap;
        }
        
        var arguments = launchConfig.Arguments;
        foreach (var (argumentKey, argumentConfig) in launchConfig.ArgumentConfig)
        {
            var replaceValue = argumentConfig.Value;
            if (fullArgMap.TryGetValue(argumentKey, out var profileArgumentValue))
            {
                replaceValue = profileArgumentValue;
            }

            arguments = arguments.Replace($"{{{argumentKey}}}", replaceValue);
        }

        return arguments;
    }
    
    public static Option<T> LoadConfig<T>(string jsonFilePath) where T : notnull
    {
        try
        {
            var jsonString = File.ReadAllText(jsonFilePath);

            var jsonOptions = new JsonSerializerOptions
            {
                AllowTrailingCommas = true,
            };
            var config = JsonSerializer.Deserialize<T>(jsonString, jsonOptions);
            
            return config is null
                ? Option.None<T>()
                : Option.Some(config);
        }
        catch
        {
            return Option.None<T>();
        }
    }
}
