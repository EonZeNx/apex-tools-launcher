using ATL.Core.Config.GUI;
using ATL.GUI.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace ATL.GUI.Views;

public partial class ToolsView : MudComponentBase
{
    [Inject]
    protected ModConfigService ModConfigService { get; set; } = new();
    
    [CascadingParameter]
    public string GameId { get; set; } = "GameId";

    protected Dictionary<string, ModConfig> ModConfigs { get; set; } = [];
    protected string ModId { get; set; } = "ModId";

    protected void ModSelectionChanged(string modId)
    {
        ModId = modId;
        StateHasChanged();
    }
    
    protected void ReloadData()
    {
        var modConfigs = ModConfigService.GetGame(GameId);
        ModConfigs = modConfigs;
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
        ModConfigService.RegisterConfigReload(OnConfigReloaded);
    }

    public void Dispose()
    {
        ModConfigService.UnregisterConfigReload(OnConfigReloaded);
    }
}