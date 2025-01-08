using ATL.Core.Config.GUI;
using ATL.Core.Libraries;
using ATL.GUI.Services.Mod;
using Microsoft.AspNetCore.Components;

namespace ATL.GUI.Components;

public partial class ModProfileSelect : ComponentBase, IDisposable
{
    [Inject]
    protected IProfileConfigService? ProfileConfigService { get; set; }
    
    [Parameter]
    public string GameId { get; set; } = "GameId";
    
    [Parameter]
    public Action<string> ProfileChanged { get; set; } = s => { };
    
    protected string SelectedProfile { get; set; } = ConstantsLibrary.InvalidString;
    
    protected Dictionary<string, ProfileConfig> ProfileConfigs { get; set; } = [];

    protected void OnProfileChanged(string value)
    {
        SelectedProfile = value;
        ProfileChanged(value);
        StateHasChanged();
    }
    
    protected void ReloadData()
    {
        if (ProfileConfigService is null)
        {
            return;
        }
        
        ProfileConfigs = ProfileConfigService.GetAllFromGame(GameId);
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
        ProfileConfigService?.RegisterConfigReload(OnConfigReloaded);
    }

    public void Dispose()
    {
        ProfileConfigService?.UnregisterConfigReload(OnConfigReloaded);
    }
}