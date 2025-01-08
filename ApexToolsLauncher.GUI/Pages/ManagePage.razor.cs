﻿using ApexToolsLauncher.Core.Config.GUI;
using ApexToolsLauncher.Core.Libraries;
using ApexToolsLauncher.GUI.Services.App;
using ApexToolsLauncher.GUI.Services.Development;
using ApexToolsLauncher.GUI.Services.Game;
using ApexToolsLauncher.GUI.Services.Mod;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace ApexToolsLauncher.GUI.Pages;

public partial class ManagePage : MudComponentBase, IDisposable
{
    [Inject]
    protected IAppStateService? AppStateService { get; set; }
    
    [Inject]
    protected IGameConfigService? GameConfigService { get; set; }
    
    [Inject]
    protected IModConfigService? ModConfigService { get; set; }
    
    [Inject]
    protected ILogService? LogService { get; set; }
    
    protected string GameId { get; set; } = ConstantsLibrary.InvalidString;
    protected string ProfileId { get; set; } = ConstantsLibrary.InvalidString;
    protected string ModId { get; set; } = ConstantsLibrary.InvalidString;


    protected void OnGameChanged(string gameId)
    {
        GameId = gameId;
    }
    
    protected void OnProfileChanged(string profileId)
    {
        ProfileId = profileId;
    }
    
    protected void OnModChanged(string modId)
    {
        ModId = modId;
        StateHasChanged();
    }
    
    protected async void OnConfigReloaded()
    {
        if (AppStateService is null || ModConfigService is null)
        {
            return;
        }
        
        GameId = AppStateService.GetLastGameId();
        if (!ModConfigService.Contains(GameId, ModId))
        {
            ModId = ConstantsLibrary.InvalidString;
        }
        
        await InvokeAsync(StateHasChanged);
    }
    
    protected override void OnInitialized()
    {
        AppStateService?.RegisterOnReload(OnConfigReloaded);
        GameConfigService?.RegisterOnReload(OnConfigReloaded);
        ModConfigService?.RegisterOnReload(OnConfigReloaded);
    }

    public void Dispose()
    {
        AppStateService?.UnregisterOnReload(OnConfigReloaded);
        GameConfigService?.UnregisterOnReload(OnConfigReloaded);
        ModConfigService?.UnregisterOnReload(OnConfigReloaded);
    }
}
