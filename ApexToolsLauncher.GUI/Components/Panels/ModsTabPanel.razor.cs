using ApexToolsLauncher.Core.Config.GUI;
using ApexToolsLauncher.Core.Libraries;
using ApexToolsLauncher.GUI.Services.App;
using ApexToolsLauncher.GUI.Services.Mod;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace ApexToolsLauncher.GUI.Components.Panels;

public partial class ModsTabPanel : MudComponentBase, IDisposable
{
    [Inject]
    protected IAppStateService? AppStateService { get; set; }
    
    [Inject]
    protected IProfileConfigService? ProfileConfigService { get; set; }
    
    [Inject]
    protected IModConfigService? ModConfigService { get; set; }
    
    [Parameter]
    public string GameId { get; set; } = ConstantsLibrary.InvalidString;
    
    [Parameter]
    public string ProfileId { get; set; } = ConstantsLibrary.InvalidString;

    [Parameter]
    public string ModId { get; set; } = ConstantsLibrary.InvalidString;
    
    [Parameter]
    public Action<string> ModChanged { get; set; } = s => { };
    
    public string SelectedVersion { get; set; } = ConstantsLibrary.InvalidString;
    
    protected ProfileConfig ProfileConfig { get; set; } = new();
    protected Dictionary<string, ModConfig> ModConfigs { get; set; } = [];
    public ModConfig ModConfig { get; set; } = new();

    protected void ListValueChanged(string? value)
    {
        if (value is null) return;

        ModId = value;
        ModChanged(value);
        StateHasChanged();
    }
    
    protected void TrySelectFirstVersion()
    {
        if (ModConfig.Versions.Count == 0) return;
        
        var version = ModConfig.Versions.Keys.First();
        SelectedVersion = version;
    }
    
    protected void ReloadData()
    {
        if (AppStateService is null) return;
        if (ProfileConfigService is null) return;
        if (ModConfigService is null) return;
        
        ModConfigs = ModConfigService.GetAllFromGame(GameId);
        ModConfig = ModConfigService.Get(GameId, ModId);

        var profileId = AppStateService.GetLastProfileId(GameId);
        ProfileConfig = ProfileConfigService.Get(GameId, profileId);
        
        if (ProfileConfig.ModConfigs.TryGetValue(ModId, out var version))
        {
            SelectedVersion = version;
        }
        else
        {
            TrySelectFirstVersion();
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
        AppStateService?.RegisterOnReload(OnConfigReloaded);
        ProfileConfigService?.RegisterOnReload(OnConfigReloaded);
        ModConfigService?.RegisterOnReload(OnConfigReloaded);
    }

    public void Dispose()
    {
        AppStateService?.UnregisterOnReload(OnConfigReloaded);
        ProfileConfigService?.UnregisterOnReload(OnConfigReloaded);
        ModConfigService?.UnregisterOnReload(OnConfigReloaded);
    }
}