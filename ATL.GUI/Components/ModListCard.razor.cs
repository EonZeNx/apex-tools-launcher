using ATL.Core.Config.GUI;
using ATL.GUI.Services;
using ATL.GUI.Services.Mod;
using Microsoft.AspNetCore.Components;

namespace ATL.GUI.Components;

public partial class ModListCard : ComponentBase, IDisposable
{
    [Inject]
    protected IModConfigService? ModConfigService { get; set; }
    
    [Parameter]
    public string GameId { get; set; } = "GameId";
    
    [Parameter]
    public Action<string> SelectionChanged { get; set; } = s => { };
    
    [Parameter]
    public string Height { get; set; } = "20rem";

    protected string ModId { get; set; } = "ModId";
    
    protected Dictionary<string, ModConfig> ModConfigs { get; set; } = [];

    protected void ListValueChanged(string? value)
    {
        if (value is null)
        {
            return;
        }
        
        if (value == ModId)
        {
            return;
        }
        
        SelectionChanged(value);
    }
    
    protected void ReloadData()
    {
        ModConfigs = ModConfigService?.GetAllFromGame(GameId) ?? [];
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
        ModConfigService?.RegisterConfigReload(OnConfigReloaded);
    }

    public void Dispose()
    {
        ModConfigService?.UnregisterConfigReload(OnConfigReloaded);
    }
}