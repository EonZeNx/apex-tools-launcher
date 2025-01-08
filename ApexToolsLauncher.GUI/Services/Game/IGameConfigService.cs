using ApexToolsLauncher.Core.Config.GUI;

namespace ApexToolsLauncher.GUI.Services.Game;

public interface IGameConfigService
{
    void Load(string gameId);
    Task LoadAsync(string gameId);
    
    void LoadAll();
    Task LoadAllAsync();

    GameConfig Get(string gameId);
    Task<GameConfig> GetAsync(string gameId);
    
    GameConfig GetOrLoad(string gameId);
    Task<GameConfig> GetOrLoadAsync(string gameId);

    Dictionary<string, GameConfig> GetAll();
    Task<Dictionary<string, GameConfig>> GetAllAsync();
    
    void RegisterOnReload(Action action);
    void UnregisterOnReload(Action action);
}