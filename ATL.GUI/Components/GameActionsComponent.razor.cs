using ATL.Core.Config.GUI;
using ATL.GUI.Services.Game;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace ATL.GUI.Components;

public partial class GameActionsComponent : MudComponentBase, IDisposable
{
    [Inject]
    protected IGameConfigService? GameConfigService { get; set; }
    
    [Parameter]
    public string GameId { get; set; } = "GameId";
    
    protected GameConfig GameConfig { get; set; } = new();

    protected void OnProfileChanged(string profileId)
    {
        
    }
    
    protected void ReloadData()
    {
        GameConfig = GameConfigService?.GetOrLoad(GameId) ?? new GameConfig();
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
        GameConfigService?.RegisterConfigReload(OnConfigReloaded);
    }

    public void Dispose()
    {
        GameConfigService?.UnregisterConfigReload(OnConfigReloaded);
    }
}