using System.Text.Json;
using ATL.Core.Config;
using ATL.Core.Libraries;
using Microsoft.AspNetCore.Components;

namespace ATL.GUI.Services;

public class AppStateService
{
    public static string AppStateFileName = "atl_state";
    
    protected AppState AppState { get; set; } = new()
    {
        LastPage = "home"
    };
    protected LogService LogService { get; set; }
    protected NavigationManager? NavigationManager { get; set; }
    
    public AppStateService(LogService? logService = null, NavigationManager? navigationManager = null)
    {
        logService ??= new LogService();
        LogService = logService;
        
        NavigationManager = navigationManager;

        Load();
    }

    public AppState Load()
    {
        var statePath = AppStatePath();
        if (!File.Exists(statePath))
        {
            try
            {
                Directory.CreateDirectory(BasePath());

                var defaultState = new AppState();
                var jsonSerializeOptions = new JsonSerializerOptions
                {
                    WriteIndented = true
                };
                var defaultAppConfigJson = JsonSerializer.Serialize(defaultState, jsonSerializeOptions);

                File.WriteAllText(statePath, defaultAppConfigJson);
            }
            catch (Exception e)
            {
                LogService.Error(e.Message);
            }
        }
        
        var optionState = ConfigLibrary.LoadConfig<AppState>(statePath);
        if (!optionState.IsSome(out var appState))
        {
            LogService.Error("Failed to load");
            return AppState;
        }
        
        lock (AppState)
        {
            AppState = appState;
        }
        
        return AppState;
    }

    public bool GoToLatestPage()
    {
        if (NavigationManager is null)
        {
            LogService.Error("Navigation manager is null");
            return false;
        }
        
        var relativePath = NavigationManager.ToBaseRelativePath(NavigationManager.Uri);
        if (AppState.LastPage == relativePath)
        {
            return false;
        }
        
        NavigationManager.NavigateTo(AppState.LastPage);
        return true;
    }

    public void SetLatestPage()
    {
        if (NavigationManager is null)
        {
            LogService.Error("Navigation manager is null");
            return;
        }
        
        var relativePath = NavigationManager.ToBaseRelativePath(NavigationManager.Uri);
        AppState.LastPage = relativePath;

        Save();
    }

    public void Save()
    {
        var configPath = AppStatePath();
        if (!File.Exists(configPath))
        {
            Directory.CreateDirectory(AppStatePath());
        }

        var jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        var jsonString = JsonSerializer.Serialize(AppState, jsonOptions);
        File.WriteAllText(configPath, jsonString);
    }


    public string BasePath() => AppDomain.CurrentDomain.BaseDirectory;
    public string AppStatePath() => Path.Join(BasePath(), $"{AppStateFileName}.json");
}