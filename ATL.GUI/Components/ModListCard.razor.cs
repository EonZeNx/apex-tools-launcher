using ATL.Core.Config.GUI;
using ATL.GUI.Services;
using Microsoft.AspNetCore.Components;

namespace ATL.GUI.Components;

public partial class ModListCard : ComponentBase, IDisposable
{
    [Inject]
    protected ModConfigService ModConfigService { get; set; } = new();
    
    [CascadingParameter]
    public string GameId { get; set; } = "GameId";
    
    [Parameter]
    public string ModId { get; set; } = "ModId";
    
    [Parameter]
    public Action<string> ModSelectionChanged { get; set; } = s => { };
    
    [Parameter]
    public string Height { get; set; } = "20rem";

    protected Dictionary<string, ModConfig> ModConfigs { get; set; } = [];

    protected void ListValueChanged(object value)
    {
        var result = (string) value;

        if (result == ModId)
        {
            return;
        }
        
        ModSelectionChanged(result);
    }
    
    protected void ReloadData()
    {
        ModConfigs = ModConfigService.GetGame(GameId);
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