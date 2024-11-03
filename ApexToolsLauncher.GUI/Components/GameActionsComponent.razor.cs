using ApexToolsLauncher.Core.Config.GUI;
using ApexToolsLauncher.GUI.Services.App;
using ApexToolsLauncher.GUI.Services.Game;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace ApexToolsLauncher.GUI.Components;

public partial class GameActionsComponent : MudComponentBase, IDisposable
{
    [Inject]
    protected IAppStateService? AppStateService { get; set; }
    
    [Inject]
    protected IGameConfigService? GameConfigService { get; set; }
    
    [Inject]
    protected ILaunchGameService? LaunchGameService { get; set; }
    
    [Parameter]
    public string GameId { get; set; } = "GameId";
    
    [Parameter]
    public Action<string> ProfileChanged { get; set; } = s => { };
    
    protected GameConfig GameConfig { get; set; } = new();

    protected void OnLaunch()
    {
        if (LaunchGameService is null) return;
        
        LaunchGameService.Launch(GameId);
    }
    
    protected void OnProfileChanged(string profileId)
    {
        ProfileChanged(profileId);

        if (AppStateService is null) return;

        AppStateService.SetLastProfileId(GameId, profileId);
    }
    
    protected void ReloadData()
    {
        if (GameConfigService is null) return;

        GameConfig = GameConfigService.Get(GameId);
    }
    
    protected override async Task OnParametersSetAsync()
    {
        await Task.Run(ReloadData);
    }
    
    protected async void OnConfigReloaded()
    {
        await Task.Run(ReloadData);
        await InvokeAsync(StateHasChanged);
    }
    
    protected override void OnInitialized()
    {
        GameConfigService?.RegisterOnReload(OnConfigReloaded);
    }

    public void Dispose()
    {
        GameConfigService?.UnregisterOnReload(OnConfigReloaded);
    }
}