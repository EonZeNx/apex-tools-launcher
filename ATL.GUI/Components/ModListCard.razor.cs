using ATL.Core.Config.GUI;
using ATL.Core.Libraries;
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
    public string ModId { get; set; } = ConstantsLibrary.InvalidString;
    
    [Parameter]
    public Action<string> SelectionChanged { get; set; } = s => { };
    
    [Parameter]
    public string Height { get; set; } = "20rem";
    
    protected Dictionary<string, ModConfig> ModConfigs { get; set; } = [];

    protected void ListValueChanged(string? value)
    {
        if (value is null)
        {
            return;
        }
        
        if (string.Equals(value, ModId))
        {
            return;
        }

        ModId = value;
        SelectionChanged(value);
    }
    
    protected void ReloadData()
    {
        if (ModConfigService is null)
        {
            return;
        }
        
        ModConfigs = ModConfigService.GetAllFromGame(GameId);
        if (!ModConfigs.ContainsKey(GameId))
        {
            ModId = ModConfigs.Count != 0 ? ModConfigs.Keys.First() : ConstantsLibrary.InvalidString;
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
        ModConfigService?.RegisterOnReload(OnConfigReloaded);
    }

    public void Dispose()
    {
        ModConfigService?.UnregisterOnReload(OnConfigReloaded);
    }
}