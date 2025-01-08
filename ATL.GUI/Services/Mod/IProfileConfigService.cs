using ATL.Core.Config.GUI;

namespace ATL.GUI.Services.Mod;

public interface IProfileConfigService
{
    void Load(string gameId, string profileId);
    Task LoadAsync(string gameId, string profileId);
    
    void LoadAllFromGame(string gameId);
    Task LoadAllFromGameAsync(string gameId);
    
    void LoadAll();
    Task LoadAllAsync();

    ProfileConfig Get(string gameId, string profileId);
    Task<ProfileConfig> GetAsync(string gameId, string profileId);

    Dictionary<string, ProfileConfig> GetAllFromGame(string gameId);
    Task<Dictionary<string, ProfileConfig>> GetAllFromGameAsync(string gameId);

    void Save(string gameId, string profileId, ProfileConfig profileConfig);
    Task SaveAsync(string gameId, string profileId, ProfileConfig profileConfig);
    
    void Delete(string gameId, string profileId);
    Task DeleteAsync(string gameId, string profileId);
    
    void RegisterConfigReload(Action action);
    void UnregisterConfigReload(Action action);
}