using ApexToolsLauncher.Core.Config.GUI;
using ApexToolsLauncher.Core.Libraries;
using ApexToolsLauncher.GUI.Dialogs;
using ApexToolsLauncher.GUI.Services.Mod;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace ApexToolsLauncher.GUI.Components;

public partial class ModProfileSelect : MudComponentBase, IDisposable
{
    [Inject]
    public IDialogService? DialogService { get; set; }
    
    [Inject]
    protected IProfileConfigService? ProfileConfigService { get; set; }
    
    [Parameter]
    public string GameId { get; set; } = "GameId";
    
    [Parameter]
    public Action<string> ProfileChanged { get; set; } = s => { };
    
    [Parameter]
    public bool HideManage { get; set; } = false;
    
    [Parameter]
    public bool HideDelete { get; set; } = false;
    
    protected string SelectedProfile { get; set; } = ConstantsLibrary.InvalidString;
    
    protected Dictionary<string, ProfileConfig> ProfileConfigs { get; set; } = [];

    protected void OnProfileChanged(string value)
    {
        SelectedProfile = value;
        ProfileChanged(value);
        StateHasChanged();
    }
    
    protected async void ManageProfiles()
    {
        if (DialogService is null) return;
        
        var parameters = new DialogParameters
        {
            { "GameId", GameId },
            { "ProfileId", SelectedProfile }
        };

        var dialog = await DialogService.ShowAsync<ManageProfileDialog>("Manage", parameters);
        var dialogResult = await dialog.Result;
        
        if (dialogResult is null) return;
        if (dialogResult.Canceled) return;
        
        await InvokeAsync(StateHasChanged);
    }
    
    protected void ReloadData()
    {
        if (ProfileConfigService is null)
        {
            return;
        }
        
        ProfileConfigs = ProfileConfigService.GetAllFromGame(GameId);
        if (!ProfileConfigs.ContainsKey(SelectedProfile) && !ConstantsLibrary.IsStringInvalid(SelectedProfile))
        {
            SelectedProfile = ConstantsLibrary.InvalidString;
            ProfileChanged(SelectedProfile);
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
        ProfileConfigService?.RegisterOnReload(OnConfigReloaded);
    }

    public void Dispose()
    {
        ProfileConfigService?.UnregisterOnReload(OnConfigReloaded);
    }
}