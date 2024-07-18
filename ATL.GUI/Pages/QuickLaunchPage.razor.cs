using ATL.GUI.Services;
using Microsoft.AspNetCore.Components;

namespace ATL.GUI.Pages;

public partial class QuickLaunchPage : ComponentBase, IDisposable
{
    [Inject]
    protected AppStateService AppStateService { get; set; } = new();
    
    [Inject]
    protected AppConfigService AppConfigService { get; set; } = new();
    
    [Inject]
    protected GameConfigService GameConfigService { get; set; } = new();
    
    [Inject]
    protected ProfileConfigService ProfileConfigService { get; set; } = new();
    
    protected void OnConfigReloaded()
    {
        InvokeAsync(StateHasChanged);
    }
    
    protected override void OnInitialized()
    {
        AppStateService.SetLatestPage();
        AppConfigService.Load();
        
        AppConfigService.RegisterConfigReload(OnConfigReloaded);
        GameConfigService.RegisterConfigReload(OnConfigReloaded);
        ProfileConfigService.RegisterConfigReload(OnConfigReloaded);
    }

    public void Dispose()
    {
        AppConfigService.UnregisterConfigReload(OnConfigReloaded);
        GameConfigService.UnregisterConfigReload(OnConfigReloaded);
        ProfileConfigService.UnregisterConfigReload(OnConfigReloaded);
    }
}