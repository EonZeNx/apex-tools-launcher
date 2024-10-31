using ATL.Core.Config.GUI;
using ATL.Core.Libraries;
using ATL.GUI.Dialogs;
using ATL.GUI.Services;
using ATL.GUI.Services.Game;
using ATL.GUI.Services.Mod;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace ATL.GUI.Components;

public partial class GameProfileComponent : MudComponentBase, IDisposable
{
    [Inject]
    protected IGameConfigService? GameConfigService { get; set; }
    
    [Inject]
    protected ProfileConfigService ProfileConfigService { get; set; } = new();
    
    [Inject]
    protected IModConfigService ModConfigService { get; set; }
    
    [Inject]
    public IDialogService DialogService { get; set; } = new DialogService();
    
    [CascadingParameter]
    public string GameId { get; set; } = "GameId";

    [Parameter]
    public bool AllowCreate { get; set; } = true;

    [Parameter]
    public bool AllowEdit { get; set; } = true;

    [Parameter]
    public string SelectorMinWidth { get; set; } = "20rem";
    
    public GameConfig GameConfig = new();
    public Dictionary<string, ProfileConfig> ProfileConfigs = [];

    protected async Task CreateProfile()
    {
        var parameters = new DialogParameters { { "GameId", GameId } };

        var dialog = await DialogService.ShowAsync<AddEditProfileDialog>("Create profile", parameters);
        var dialogResult = await dialog.Result;

        if (dialogResult.Canceled) return;
        var result = (ProfileDialogResult) dialogResult.Data;

        var config = new ProfileConfig
        {
            Title = result.Title
        };
        
        var profileId = ConstantsLibrary.CreateId(config.Title);
        ProfileConfigService.Save(GameId, profileId, config);

        // var gameConfig = GameConfigService.Get(GameId);
        // gameConfig.SelectedProfile = profileId;
        //
        // GameConfigService.Save(GameId, gameConfig);
    }
    
    protected async Task EditProfile()
    {
        var selectedProfileId = ConfigLibrary.GetSelectedProfileId(GameId);
        var parameters = new DialogParameters
        {
            { "GameId", GameId },
            { "ProfileId", selectedProfileId },
            { "Edit", true }
        };

        var dialog = await DialogService.ShowAsync<AddEditProfileDialog>("Edit profile", parameters);
        var dialogResult = await dialog.Result;

        if (dialogResult.Canceled) return;
        var result = (ProfileDialogResult) dialogResult.Data;

        if (result.Delete)
        {
            ProfileConfigService.Delete(GameId, selectedProfileId);
            return;
        }
        
        // var gameConfig = GameConfigService.Get(GameId);
        // var profileConfig = ProfileConfigService.Get(GameId, gameConfig.SelectedProfile);
        // profileConfig.Title = result.Title;
        //
        // var profileId = ConstantsLibrary.CreateId(profileConfig.Title);
        // ProfileConfigService.Update(GameId, selectedProfileId, profileConfig);
        //
        // gameConfig.SelectedProfile = profileId;
        // GameConfigService.Save(GameId, gameConfig);
    }

    protected bool ProfileInvalid()
    {
        var result = false;

        // result |= ConstantsLibrary.IsStringInvalid(GameConfig.SelectedProfile);
        // result |= !ProfileConfigService.ProfileExists(GameId, GameConfig.SelectedProfile);
        
        return result;
    }
    
    protected void OnProfileChanged(string profileId)
    {
        // var gameConfig = GameConfigService.Get(GameId);
        // gameConfig.SelectedProfile = profileId;
        //
        // GameConfigService.Save(GameId, gameConfig);
    }

    protected void ReloadData()
    {
        // GameConfig = GameConfigService.Get(GameId);
        // ProfileConfigs = ProfileConfigService.GetGame(GameId);
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
        GameConfigService?.RegisterOnReload(OnConfigReloaded);
        ProfileConfigService.RegisterConfigReload(OnConfigReloaded);
        ModConfigService.RegisterOnReload(OnConfigReloaded);
    }
    
    public void Dispose()
    {
        GameConfigService?.UnregisterOnReload(OnConfigReloaded);
        ProfileConfigService.UnregisterConfigReload(OnConfigReloaded);
        ModConfigService.UnregisterOnReload(OnConfigReloaded);
    }
}