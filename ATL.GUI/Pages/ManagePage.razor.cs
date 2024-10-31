using ATL.Core.Config.GUI;
using ATL.Core.Libraries;
using ATL.GUI.Services.App;
using ATL.GUI.Services.Development;
using ATL.GUI.Services.Game;
using ATL.GUI.Services.Mod;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace ATL.GUI.Pages;

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


    protected void SelectedGameChanged(string gameId)
    {
        GameId = gameId;
    }
    
    protected void SelectedProfileChanged(string profileId)
    {
        ProfileId = profileId;
    }
    
    protected void SelectedModChanged(string modId)
    {
        ModId = modId;
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
            var allFromGame = await ModConfigService.GetAllFromGameAsync(GameId);
            ModId = allFromGame.Count != 0 ? allFromGame.Keys.First() : ConstantsLibrary.InvalidString;
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
