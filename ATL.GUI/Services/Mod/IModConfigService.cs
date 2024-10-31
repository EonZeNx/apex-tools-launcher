﻿using ATL.Core.Config.GUI;

namespace ATL.GUI.Services.Mod;

public interface IModConfigService
{
    void Load(string gameId, string modId);
    Task LoadAsync(string gameId, string modId);
    
    void LoadAllFromGame(string gameId);
    Task LoadAllFromGameAsync(string gameId);
    
    void LoadAll();
    Task LoadAllAsync();

    ModConfig Get(string gameId, string modId);
    Task<ModConfig> GetAsync(string gameId, string modId);

    Dictionary<string, ModConfig> GetAllFromGame(string gameId);
    Task<Dictionary<string, ModConfig>> GetAllFromGameAsync(string gameId);
    
    void RegisterConfigReload(Action action);
    void UnregisterConfigReload(Action action);
}