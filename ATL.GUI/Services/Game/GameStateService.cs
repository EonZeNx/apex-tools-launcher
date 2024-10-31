using ATL.Core.Config.GUI;
using ATL.GUI.Services.Development;

namespace ATL.GUI.Services.Game;

public class GameStateService : IGameStateService
{
    protected Dictionary<string, GameState> GameStates { get; set; } = [];
    protected event Action StateUpdated = () => { };
    protected ILogService? LogService { get; set; }

    public GameStateService(ILogService? logService = null)
    {
        LogService = logService;
    }

    
    public void Save(string gameId, GameState state)
    {
        throw new NotImplementedException();
    }

    public Task SaveAsync(string gameId, GameState state)
    {
        throw new NotImplementedException();
    }


    public void Load(string gameId)
    {
        throw new NotImplementedException();
    }

    public Task LoadAsync(string gameId)
    {
        throw new NotImplementedException();
    }

    
    public void LoadAll()
    {
        throw new NotImplementedException();
    }

    public Task LoadAllAsync()
    {
        throw new NotImplementedException();
    }

    
    public GameState Get(string gameId)
    {
        throw new NotImplementedException();
    }

    public Task<GameState> GetAsync(string gameId)
    {
        throw new NotImplementedException();
    }

    
    public Dictionary<string, GameState> GetAll()
    {
        throw new NotImplementedException();
    }

    public Task<Dictionary<string, GameState>> GetAllAsync()
    {
        throw new NotImplementedException();
    }


    public void RegisterOnReload(Action action)
    {
        LogService?.Debug("Adding to reload event");
        StateUpdated += action;
    }

    public void UnregisterOnReload(Action action)
    {
        LogService?.Debug("Removing from reload event");
        StateUpdated -= action;
    }
}