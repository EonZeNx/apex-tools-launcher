using ATL.Core.Config.GUI;
using ATL.GUI.Services;
using ATL.GUI.Services.Game;
using ATL.GUI.Services.Mod;
using Microsoft.AspNetCore.Components;

namespace ATL.GUI.Components;

public partial class ProfileLaunchArgumentsComponent : ComponentBase, IDisposable
{
    [Inject]
    protected GameConfigService GameConfigService { get; set; } = new();
    
    [Inject]
    protected ProfileConfigService ProfileConfigService { get; set; } = new();
    
    [CascadingParameter]
    public string GameId { get; set; } = "GameId";
    
    public GameConfig GameConfig { get; set; } = new();
    
    protected void ReloadData()
    {
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
        GameConfigService.RegisterOnReload(OnConfigReloaded);
        ProfileConfigService.RegisterOnReload(OnConfigReloaded);
    }
    
    public void Dispose()
    {
        GameConfigService.UnregisterOnReload(OnConfigReloaded);
        ProfileConfigService.UnregisterOnReload(OnConfigReloaded);
    }
}