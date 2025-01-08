using System.Globalization;
using ApexToolsLauncher.GUI.Dialogs;
using ApexToolsLauncher.Core.Config.GUI;
using ApexToolsLauncher.Core.Libraries;
using ApexToolsLauncher.GUI.Libraries;
using ApexToolsLauncher.GUI.Services.Development;
using ApexToolsLauncher.GUI.Services.Game;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using MudBlazor;

namespace ApexToolsLauncher.GUI.Components;

public partial class NavMenu : ComponentBase
{
    [Inject]
    protected NavigationManager? NavigationManager { get; set; }
    
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

    protected string PageTitle { get; set; } = ConstantsLibrary.AppTitle;
    
    protected bool DrawerOpen { get; set; }
    protected void ToggleDrawer() => DrawerOpen = !DrawerOpen;
    
    protected async void OpenSettings()
    {
        var dialog = await DialogService.ShowAsync<SettingsDialog>("Settings");
        var dialogResult = await dialog.Result;
    }
    
    protected void OnLocationChanged(object? sender, LocationChangedEventArgs args)
    {
        if (NavigationManager is null) return;
        
        var relativePath = NavigationManager.ToBaseRelativePath(NavigationManager.Uri);
        if (relativePath.Length == 0)
        {
            PageTitle = ConstantsLibrary.AppTitle;
            return;
        }
        
        if (MauiConstantsLibrary.PageInfos.TryGetValue(relativePath, out var info))
        {
            PageTitle = info.Title;
            return;
        }

        var textInfo = CultureInfo.CurrentCulture.TextInfo;
        PageTitle = textInfo.ToTitleCase(relativePath.Replace("_", " "));
    }
    
    protected void ReloadData()
    {
        GameConfigs = GameConfigService?.GetAll() ?? [];
    }
    
    protected override async Task OnParametersSetAsync()
    {
        await Task.Run(ReloadData);
        
        if (NavigationManager is not null)
        {
            NavigationManager.LocationChanged += OnLocationChanged;
        }
    }
}