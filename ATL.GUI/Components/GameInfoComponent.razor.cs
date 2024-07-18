using ATL.Core.Config.GUI;
using ATL.GUI.Services;
using Microsoft.AspNetCore.Components;

namespace ATL.GUI.Components;

public partial class GameInfoComponent : ComponentBase, IDisposable
{
    [Inject]
    protected GameConfigService GameConfigService { get; set; } = new();
    
    [CascadingParameter]
    public string GameId { get; set; } = "GameId";
    
    [Parameter]
    public bool UseProfileComponent { get; set; } = true;
    
    protected GameConfig GameConfig { get; set; } = new();
    
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
        GameConfigService.RegisterConfigReload(OnConfigReloaded);
    }

    public void Dispose()
    {
        GameConfigService.UnregisterConfigReload(OnConfigReloaded);
    }
}