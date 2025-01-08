using ApexToolsLauncher.Core.Config.GUI;
using ApexToolsLauncher.Core.Libraries;
using ApexToolsLauncher.GUI.Services.App;
using ApexToolsLauncher.GUI.Services.Game;
using ApexToolsLauncher.GUI.Services.Mod;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace ApexToolsLauncher.GUI.Components.Mods;

public partial class ModVersionSelect : MudComponentBase
{
    [Inject]
    protected IGameConfigService? GameConfigService { get; set; }
    
    [Inject]
    protected IModConfigService? ModConfigService { get; set; }
    
    [Parameter]
    public string GameId { get; set; } = ConstantsLibrary.InvalidString;
    
    [Parameter]
    public string ModId { get; set; } = ConstantsLibrary.InvalidString;
    
    [Parameter]
    public string Version { get; set; } = ConstantsLibrary.InvalidString;
    
    [Parameter]
    public Action<string> VersionChanged { get; set; } = s => { };

    protected ModConfig ModConfig { get; set; } = new();

    protected void OnVersionChanged(string version)
    {
        VersionChanged(version);
    }
    
    protected void ReloadData()
    {
        if (GameConfigService is null) return;
        if (ModConfigService is null) return;
        
        ModConfig = ModConfigService.Get(GameId, ModId);
    }
    
    protected override async Task OnParametersSetAsync()
    {
        await Task.Run(ReloadData);
    }
}