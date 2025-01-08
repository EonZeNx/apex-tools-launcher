﻿using ATL.Core.Config.GUI;

namespace ATL.GUI.Services.Game;

public interface IGameStateService
{
    void Save(string gameId, GameState state);
    Task SaveAsync(string gameId, GameState state);
    
    void Load(string gameId);
    Task LoadAsync(string gameId);
    
    void LoadAll();
    Task LoadAllAsync();

    GameState Get(string gameId);
    Task<GameState> GetAsync(string gameId);

    Dictionary<string, GameState> GetAll();
    Task<Dictionary<string, GameState>> GetAllAsync();
    
    void RegisterStateUpdated(Action action);
    void UnregisterStateUpdated(Action action);
}