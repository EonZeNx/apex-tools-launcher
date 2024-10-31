using ATL.Core.Config.GUI;
using ATL.Core.Libraries;
using ATL.GUI.Services.Mod;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace ATL.GUI.Components;

public partial class ModListCard : MudComponentBase, IDisposable
{
    [Inject]
    protected IModConfigService? ModConfigService { get; set; }
    
    [Parameter]
    public string GameId { get; set; } = "GameId";

    [Parameter]
    public string ModId { get; set; } = ConstantsLibrary.InvalidString;
    
    [Parameter]
    public Action<string> ModChanged { get; set; } = s => { };
    
    protected Dictionary<string, ModConfig> ModConfigs { get; set; } = [];

    protected void ListValueChanged(string? value)
    {
        if (value is null)
        {
            return;
        }
        
        ModChanged(value);
    }
    
    protected void ReloadData()
    {
        if (ModConfigService is null)
        {
            return;
        }
        
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
        ModConfigService?.RegisterOnReload(OnConfigReloaded);
    }

    public void Dispose()
    {
        ModConfigService?.UnregisterOnReload(OnConfigReloaded);
    }
}