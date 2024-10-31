using ATL.GUI.Dialogs;
using ATL.Core.Config.GUI;
using ATL.GUI.Services.Development;
using ATL.GUI.Services.Game;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace ATL.GUI.Components;

public partial class NavMenu : ComponentBase
{
    [Inject]
    protected IGameConfigService? GameConfigService { get; set; }
    
    [Inject]
    protected ILogService? LogService { get; set; }
    
    [Inject]
    public IDialogService DialogService { get; set; } = new DialogService();

    [CascadingParameter]
    protected MudTheme Theme { get; set; } = new();
    
    [Parameter]
    public bool UsingDarkMode { get; set; } = true;

    [Parameter]
    public Action ToggleDarkMode { get; set; } = () => { };

    public Dictionary<string, GameConfig> GameConfigs = [];
    
    protected bool DrawerOpen { get; set; }
    protected void ToggleDrawer() => DrawerOpen = !DrawerOpen;
    
    protected async void OpenSettings()
    {
        var dialog = await DialogService.ShowAsync<SettingsDialog>("Settings");
        var dialogResult = await dialog.Result;
    }
    
    protected void ReloadData()
    {
        GameConfigs = GameConfigService?.GetAll() ?? [];
    }
    
    protected override async Task OnParametersSetAsync()
    {
        await Task.Run(ReloadData);
    }
}