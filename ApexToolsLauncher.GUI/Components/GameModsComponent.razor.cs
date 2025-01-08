using ApexToolsLauncher.Core.Config.GUI;
using ApexToolsLauncher.GUI.Services;
using ApexToolsLauncher.GUI.Services.Game;
using ApexToolsLauncher.GUI.Services.Mod;
using Microsoft.AspNetCore.Components;

namespace ApexToolsLauncher.GUI.Components;

public partial class GameModsComponent : ComponentBase, IDisposable
{
    [Inject]
    protected GameConfigService GameConfigService { get; set; } = new();
    
    [Inject]
    protected ProfileConfigService ProfileConfigService { get; set; } = new();
    
    [Inject]
    protected ModConfigService ModConfigService { get; set; } = new();
    
    [CascadingParameter]
    public string GameId { get; set; } = "GameId";
    
    [Parameter]
    public string ProfileId { get; set; } = "ProfileId";
    
    public GameConfig GameConfig { get; set; } = new();
    public ProfileConfig ProfileConfig { get; set; } = new();
    public Dictionary<string, ModConfig> ModConfigs { get; set; } = [];
    
    protected void ReloadData()
    {
        GameConfig = GameConfigService.Get(GameId);
        // ProfileConfig = ProfileConfigService.Get(GameId, GameConfig.SelectedProfile);
        ModConfigs = ModConfigService.GetAllFromGame(GameId);
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
        ModConfigService.RegisterOnReload(OnConfigReloaded);
    }
    
    public void Dispose()
    {
        GameConfigService.UnregisterOnReload(OnConfigReloaded);
        ProfileConfigService.UnregisterOnReload(OnConfigReloaded);
        ModConfigService.UnregisterOnReload(OnConfigReloaded);
    }
}