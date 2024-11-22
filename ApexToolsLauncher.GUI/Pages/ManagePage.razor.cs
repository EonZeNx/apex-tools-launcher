using ApexToolsLauncher.Core.Config.GUI;
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
    protected IProfileConfigService? ProfileConfigService { get; set; }
    
    [Inject]
    protected IModConfigService? ModConfigService { get; set; }
    
    [Inject]
    protected ILogService? LogService { get; set; }
    
    protected string GameId { get; set; } = ConstantsLibrary.InvalidString;
    protected string ProfileId { get; set; } = ConstantsLibrary.InvalidString; // todo: is not updated at launch
    protected string ModId { get; set; } = ConstantsLibrary.InvalidString;

    protected GameConfig GameConfig { get; set; } = new();


    protected void OnGameChanged(string gameId)
    {
        if (GameConfigService is null) return;
        
        GameId = gameId;
        GameConfig = GameConfigService.Get(gameId);
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
        ProfileConfigService?.RegisterOnReload(OnConfigReloaded);
        ModConfigService?.RegisterOnReload(OnConfigReloaded);
    }

    public void Dispose()
    {
        AppStateService?.UnregisterOnReload(OnConfigReloaded);
        GameConfigService?.UnregisterOnReload(OnConfigReloaded);
        ProfileConfigService?.UnregisterOnReload(OnConfigReloaded);
        ModConfigService?.UnregisterOnReload(OnConfigReloaded);
    }
}
