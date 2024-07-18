using ATL.Core.Config.GUI;
using ATL.GUI.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace ATL.GUI.Pages;

public partial class GamePage : MudComponentBase, IDisposable
{
    [Inject]
    public IDialogService DialogService { get; set; } = new DialogService();
    
    [Inject]
    protected AppStateService AppStateService { get; set; } = new();
    
    [Inject]
    protected AppConfigService AppConfigService { get; set; } = new();
    
    [Inject]
    protected GameConfigService GameConfigService { get; set; } = new();
    
    [Inject]
    protected ProfileConfigService ProfileConfigService { get; set; } = new();
    
    [Inject]
    protected ModConfigService ModConfigService { get; set; } = new();
    
    [Parameter]
    public string GameId { get; set; } = "GameId";

    protected int TabIndex { get; set; } = 0;
    public GameConfig GameConfig { get; set; } = new();
    
    protected void ReloadData()
    {
        GameConfig = GameConfigService.Get(GameId);
    }
    
    protected override async Task OnParametersSetAsync()
    {
        AppStateService.SetLatestPage();
        
        await AppConfigService.LoadAsync();
        await GameConfigService.LoadAsync(GameId);
        ProfileConfigService.LoadGame(GameId);
        ModConfigService.LoadGame(GameId);
        
        await Task.Run(ReloadData);
    }
    
    protected async void OnConfigReloaded()
    {
        await Task.Run(ReloadData);
        await InvokeAsync(StateHasChanged);
    }
    
    protected override void OnInitialized()
    {
        AppConfigService.RegisterConfigReload(OnConfigReloaded);
        GameConfigService.RegisterConfigReload(OnConfigReloaded);
        ProfileConfigService.RegisterConfigReload(OnConfigReloaded);
        ModConfigService.RegisterConfigReload(OnConfigReloaded);
    }

    public void Dispose()
    {
        AppConfigService.UnregisterConfigReload(OnConfigReloaded);
        GameConfigService.UnregisterConfigReload(OnConfigReloaded);
        ProfileConfigService.UnregisterConfigReload(OnConfigReloaded);
        ModConfigService.UnregisterConfigReload(OnConfigReloaded);
    }
}
