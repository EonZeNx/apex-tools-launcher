using System.Text.Json;
using ATL.Core.Config;
using ATL.Core.Libraries;
using ATL.GUI.Services.Development;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;

namespace ATL.GUI.Services.App;

public class AppStateService : IAppStateService
{
    protected AppState AppState { get; set; } = new();
    protected event Action StateReloaded = () => { };
    
    protected ILogService? LogService { get; set; }
    protected NavigationManager? NavigationManager { get; set; }
    
    public AppStateService(ILogService? logService = null, NavigationManager? navigationManager = null)
    {
        LogService = logService;
        NavigationManager = navigationManager;

        if (NavigationManager is not null)
        {
            NavigationManager.LocationChanged += OnLocationChanged;
        }

        Load();
        GoToLastPage();
    }

    protected void OnLocationChanged(object? sender, LocationChangedEventArgs args)
    {
        if (NavigationManager is null || LogService is null)
        {
            return;
        }
        
        SetLastPage();
    }

    
    public void Load()
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
                LogService?.Error(e.Message);
            }
        }
        
        var optionState = ConfigLibrary.LoadConfig<AppState>(statePath);
        if (!optionState.IsSome(out var appState))
        {
            LogService?.Error("Failed to load");
            return;
        }
        
        lock (AppState)
        {
            AppState = appState;
        }
        
        StateReloaded.Invoke();
    }

    public Task LoadAsync()
    {
        var result = new Task(Load);
        return result;
    }

    
    public AppState Get()
    {
        return AppState;
    }

    public Task<AppState> GetAsync()
    {
        var result = Task.FromResult(Get());
        return result;
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
    
    public Task SaveAsync()
    {
        var result = new Task(Save);
        return result;
    }
    
    
    public void RegisterOnReload(Action action)
    {
        LogService?.Debug("Adding to reload event");
        StateReloaded += action;
    }

    public void UnregisterOnReload(Action action)
    {
        LogService?.Debug("Removing from reload event");
        StateReloaded -= action;
    }

    
    public void SetLastPage()
    {
        if (NavigationManager is null)
        {
            LogService?.Error("Navigation manager is null");
            return;
        }
        
        var relativePath = NavigationManager.ToBaseRelativePath(NavigationManager.Uri);
        AppState.LastPage = relativePath;

        Save();
        StateReloaded.Invoke();
    }
    
    public bool GoToLastPage()
    {
        if (NavigationManager is null)
        {
            LogService?.Error("Navigation manager is null");
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


    public void SetLastGameId(string gameId)
    {
        AppState.LastGameId = gameId;
        Save();
        StateReloaded.Invoke();
    }

    public string GetLastGameId()
    {
        return AppState.LastGameId;
    }

    
    public void SetLastProfileId(string gameId, string profileId)
    {
        AppState.LastProfileId[gameId] = profileId;
        Save();
        StateReloaded.Invoke();
    }

    public string GetLastProfileId(string gameId)
    {
        if (AppState.LastProfileId.TryGetValue(gameId, out var profileId))
        {
            return profileId;
        }

        return ConstantsLibrary.InvalidString;
    }


    public string BasePath() => AppDomain.CurrentDomain.BaseDirectory;
    public string AppStatePath() => Path.Join(BasePath(), $"{ConstantsLibrary.AppStateFileName}.json");
}