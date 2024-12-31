using ApexToolsLauncher.Core.Config;

namespace ApexToolsLauncher.GUI.Services.App;

public interface IAppStateService
{
    void Load();
    Task LoadAsync();
    
    AppState Get();
    Task<AppState> GetAsync();

    void Save();
    Task SaveAsync();

    void SetLastPage();
    bool GoToLastPage();

    void SetLastGameId(string gameId);
    string GetLastGameId();

    void SetLastProfileId(string gameId, string profileId);
    string GetLastProfileId(string gameId);
    
    void RegisterOnReload(Action action);
    void UnregisterOnReload(Action action);
}