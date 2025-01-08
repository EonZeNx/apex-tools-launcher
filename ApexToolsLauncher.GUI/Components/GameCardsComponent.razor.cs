using ApexToolsLauncher.Core.Libraries;
using ApexToolsLauncher.GUI.Services.App;
using ApexToolsLauncher.GUI.Services.Game;
using Microsoft.AspNetCore.Components;

namespace ApexToolsLauncher.GUI.Components;

public partial class GameCardsComponent : ComponentBase, IDisposable
{
    [Inject]
    protected IAppStateService? AppStateService { get; set; }
    
    [Inject]
    protected IGameConfigService? GameConfigService { get; set; }
    
    [Parameter]
    public string SelectedGameId { get; set; } = ConstantsLibrary.InvalidString;

    [Parameter]
    public Action<string> GameChanged { get; set; } = s => { };

    protected List<string> GameIds { get; set; } = [];

    protected void GameIdChanged(string gameId)
    {
        SelectedGameId = gameId;
        GameChanged(gameId);
        AppStateService?.SetLastGameId(gameId);
    }
    
    protected void ReloadData()
    {
        if (GameConfigService is null)
        {
            return;
        }

        GameIds = GameConfigService.GetAll().Keys.ToList();
        if (!GameIds.Contains(SelectedGameId))
        {
            GameIdChanged(GameIds.Count == 0 ? ConstantsLibrary.InvalidString : GameIds[0]);
        }
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