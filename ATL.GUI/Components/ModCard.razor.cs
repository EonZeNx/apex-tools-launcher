using ATL.Core.Config.GUI;
using ATL.Core.Libraries;
using ATL.GUI.Services;
using ATL.GUI.Services.Game;
using ATL.GUI.Services.Mod;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace ATL.GUI.Components;

public partial class ModCard : MudComponentBase, IDisposable
{
    [Inject]
    protected IGameConfigService? GameConfigService { get; set; }
    
    [Inject]
    protected IProfileConfigService? ProfileConfigService { get; set; }
    
    [Inject]
    protected IModConfigService? ModConfigService { get; set; }
    
    [Parameter]
    public string GameId { get; set; } = "GameId";
    
    [Parameter]
    public string ModId { get; set; } = "ModId";

    public string SelectedVersion { get; set; } = ConstantsLibrary.InvalidString;
    public GameConfig GameConfig { get; set; } = new();
    public ProfileConfig ProfileConfig { get; set; } = new();
    public ModConfig ModConfig { get; set; } = new();

    protected void OnVersionChanged(string version)
    {
        SelectedVersion = version;

        if (!ProfileConfig.ModConfigs.ContainsKey(ModId))
        {
            return;
        }
        
        ProfileConfig.ModConfigs[ModId] = version;
        // ProfileConfigService.Save(GameId, GameConfig.SelectedProfile, ProfileConfig);
    }
    
    protected void OnModEnabled(bool isEnabled)
    {
        if (isEnabled)
        {
            TrySelectFirstVersion();
            ProfileConfig.ModConfigs.Add(ModId, SelectedVersion);
        }
        else
        {
            ProfileConfig.ModConfigs.Remove(ModId);
        }
        
        // ProfileConfigService.Save(GameId, GameConfig.SelectedProfile, ProfileConfig);
    }

    protected void TrySelectFirstVersion()
    {
        if (!ConstantsLibrary.IsStringInvalid(SelectedVersion) || ModConfig.Versions.Count == 0)
        {
            return;
        }
        
        var version = ModConfig.Versions.Keys.First();
        SelectedVersion = version;
    }
    
    protected void ReloadData()
    {
        if (GameConfigService is null || ModConfigService is null)
        {
            return;
        }
        
        GameConfig = GameConfigService.Get(GameId);
        // ProfileConfig = ProfileConfigService.Get(GameId, GameConfig.SelectedProfile);
        ModConfig = ModConfigService.Get(GameId, ModId);

        if (ProfileConfig.ModConfigs.TryGetValue(ModId, out var version))
        {
            SelectedVersion = version;
        }
        else
        {
            TrySelectFirstVersion();
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
        GameConfigService?.RegisterOnReload(OnConfigReloaded);
        ProfileConfigService?.RegisterConfigReload(OnConfigReloaded);
        ModConfigService?.RegisterOnReload(OnConfigReloaded);
    }
    
    public void Dispose()
    {
        GameConfigService?.UnregisterOnReload(OnConfigReloaded);
        ProfileConfigService?.UnregisterConfigReload(OnConfigReloaded);
        ModConfigService?.UnregisterOnReload(OnConfigReloaded);
    }
}