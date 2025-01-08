using ApexToolsLauncher.Core.Config.GUI;
using ApexToolsLauncher.Core.Libraries;
using ApexToolsLauncher.GUI.Services.App;
using ApexToolsLauncher.GUI.Services.Mod;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace ApexToolsLauncher.GUI.Components.Panels;

public partial class ProfilesTabPanel : MudComponentBase, IDisposable
{
    [Inject]
    protected IAppStateService? AppStateService { get; set; }
    
    [Inject]
    protected IProfileConfigService? ProfileConfigService { get; set; }
    
    [Parameter]
    public string GameId { get; set; } = ConstantsLibrary.InvalidString;
    
    [Parameter]
    public string ProfileId { get; set; } = ConstantsLibrary.InvalidString;
    
    protected ProfileConfig ProfileConfig { get; set; } = new();
    protected string EditedTitle { get; set; } = string.Empty;
    protected bool InvalidTitle => 
        ProfileConfigService?.IdExists(
            GameId,
            ProfileConfigService?.ToId(EditedTitle) ?? ConstantsLibrary.InvalidString
        ) ?? true;
    protected Dictionary<string, ProfileConfig> ProfileConfigs { get; set; } = [];

    protected void ListValueChanged(string? value)
    {
        if (value is null) return;

        ProfileId = value;
        ReloadData();
        StateHasChanged();
    }

    protected void ConfirmName()
    {
        if (ProfileConfigService is null) return;

        if (ConstantsLibrary.IsStringInvalid(ProfileId))
        { // creating a new profile
            var newProfileId = ProfileConfigService.ToId(EditedTitle);
            var newProfileConfig = new ProfileConfig
            {
                Title = EditedTitle,
            };
            
            var result = ProfileConfigService.Create(GameId, newProfileId, newProfileConfig);

            if (result)
            {
                ProfileId = newProfileId;
            }
        }
        else
        {
            ProfileConfigService.Save(GameId, ProfileId, ProfileConfig);
        }
        
        ReloadData();
        StateHasChanged();
    }

    protected void RefreshEditedTitle()
    {
        EditedTitle = ProfileConfig.Title;
        StateHasChanged();
    }

    protected void ReloadData()
    {
        if (AppStateService is null) return;
        if (ProfileConfigService is null) return;
        
        ProfileConfigs = ProfileConfigService.GetAllFromGame(GameId);
        if (!ProfileConfigs.ContainsKey(ProfileId) && !ConstantsLibrary.IsStringInvalid(ProfileId))
        {
            ProfileId = ConstantsLibrary.InvalidString;
        }
        
        ProfileConfig = ProfileConfigService.Get(GameId, ProfileId);
        EditedTitle = ProfileConfig.Title;
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
    }

    public void Dispose()
    {
        AppStateService?.UnregisterOnReload(OnConfigReloaded);
        ProfileConfigService?.UnregisterOnReload(OnConfigReloaded);
    }
}