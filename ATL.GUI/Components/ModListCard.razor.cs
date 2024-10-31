using ATL.Core.Config.GUI;
using ATL.Core.Libraries;
using ATL.GUI.Services.App;
using ATL.GUI.Services.Mod;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace ATL.GUI.Components;

public partial class ModListCard : MudComponentBase, IDisposable
{
    [Inject]
    protected IAppStateService? AppStateService { get; set; }
    
    [Inject]
    protected IProfileConfigService? ProfileConfigService { get; set; }
    
    [Inject]
    protected IModConfigService? ModConfigService { get; set; }
    
    [Parameter]
    public string GameId { get; set; } = "GameId";

    [Parameter]
    public string ModId { get; set; } = ConstantsLibrary.InvalidString;
    
    [Parameter]
    public Action<string> ModChanged { get; set; } = s => { };
    
    protected string ProfileId { get; set; } = ConstantsLibrary.InvalidString;
    protected ProfileConfig ProfileConfig { get; set; } = new();
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
        if (AppStateService is null) return;
        if (ProfileConfigService is null) return;
        if (ModConfigService is null) return;
        
        ModConfigs = ModConfigService.GetAllFromGame(GameId);

        ProfileId = AppStateService.GetLastProfileId(GameId);
        ProfileConfig = ProfileConfigService.Get(GameId, ProfileId);
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