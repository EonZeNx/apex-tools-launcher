using System.Text.RegularExpressions;
using ApexToolsLauncher.Core.Config.GUI;
using ApexToolsLauncher.Core.Libraries;
using ApexToolsLauncher.GUI.Libraries;
using ApexToolsLauncher.GUI.Services.Mod;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace ApexToolsLauncher.GUI.Dialogs;

public partial class ManageProfileDialog : ComponentBase, IDisposable
{
    [Inject]
    protected IProfileConfigService? ProfileConfigService { get; set; }
    
    [CascadingParameter]
    public MudDialogInstance? MudDialog { get; set; }
    
    [Parameter]
    public string GameId { get; set; } = ConstantsLibrary.InvalidString;
    
    [Parameter]
    public string ProfileId { get; set; } = ConstantsLibrary.InvalidString;

    protected string ProfileTitle { get; set; } = ConstantsLibrary.InvalidString;
    protected string ProfileTitleAsId => ProfileTitle.ToLower().Replace(" ", "_");
    
    private string _selectedProfile = ConstantsLibrary.InvalidString;
    protected string SelectedProfile
    {
        get => _selectedProfile;
        set
        {
            _selectedProfile = value;
            OnProfileChanged(value);
        }
    }
    
    protected Dictionary<string, ProfileConfig> ProfileConfigs { get; set; } = [];

    protected bool ProfileTitleValid()
    {
        if (ProfileConfigService is null) return false;

        if (string.IsNullOrEmpty(ProfileTitleAsId))
        {
            return false;
        }

        var regexMatch = Regex.Match(ProfileTitle, MauiConstantsLibrary.IdMask, RegexOptions.None);
        if (!regexMatch.Success) return false;

        var profileConfigs = ProfileConfigService.GetAllFromGame(GameId);
        return !profileConfigs.ContainsKey(ProfileTitleAsId);
    }


    protected void Refresh()
    {
        OnProfileChanged(SelectedProfile);
        StateHasChanged();
    }

    protected void Create()
    {
        if (ProfileConfigService is null) return;

        var profileConfig = new ProfileConfig
        {
            Title = ProfileTitle
        };

        ProfileConfigService.Save(GameId, ProfileTitleAsId, profileConfig);
        SelectedProfile = ProfileTitleAsId;
    }

    protected void Delete()
    {
        if (ProfileConfigService is null) return;
        
        ProfileConfigService.Delete(GameId, SelectedProfile);
    }
    
    
    protected void OnProfileChanged(string value)
    {
        if (ProfileConfigService is null) return;

        if (!ConstantsLibrary.IsStringInvalid(value))
        {
            var profileConfig = ProfileConfigService.Get(GameId, SelectedProfile);
            ProfileTitle = profileConfig.Title;
        }
        else
        {
            ProfileTitle = "";
        }
    }

    protected void ReloadData()
    {
        if (ProfileConfigService is null) return;
        
        ProfileConfigs = ProfileConfigService.GetAllFromGame(GameId);
        if (ProfileConfigs.ContainsKey(SelectedProfile)) return;
        
        if (ProfileConfigs.Count != 0)
        {
            SelectedProfile = ProfileConfigs.Keys.First();
        }
        else
        {
            SelectedProfile = ConstantsLibrary.InvalidString;
        }
    }
    
    protected override async Task OnParametersSetAsync()
    {
        SelectedProfile = ProfileId;
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
    
    protected void Close() => MudDialog?.Close(DialogResult.Cancel());
    protected void Confirm() => MudDialog?.Close();
}