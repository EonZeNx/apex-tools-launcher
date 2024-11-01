using ApexToolsLauncher.Core.Config.GUI;
using ApexToolsLauncher.GUI.Services;
using ApexToolsLauncher.GUI.Services.Game;
using ApexToolsLauncher.GUI.Services.Mod;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace ApexToolsLauncher.GUI.Components;

public partial class QuickLaunchComponent : MudComponentBase
{
    [Inject]
    protected GameConfigService GameConfigService { get; set; } = new();
    
    [Inject]
    protected LaunchGameService? LaunchGameService { get; set; }
    
    [Inject]
    protected ProfileConfigService ProfileConfigService { get; set; } = new();
    
    protected Dictionary<string, GameConfig> GameConfigs = [];
    protected Dictionary<string, Dictionary<string, ProfileConfig>> GameProfileConfigs = [];

    protected void TryLaunchGame(string gameId)
    {
        LaunchGameService?.Launch(gameId);
    }
    
    protected void ReloadData()
    {
        GameConfigs = GameConfigService.GetAll();
        // GameProfileConfigs = ProfileConfigService.GetAll();
    }
    
    protected override async Task OnParametersSetAsync()
    {
        await Task.Run(ReloadData);
    }
}