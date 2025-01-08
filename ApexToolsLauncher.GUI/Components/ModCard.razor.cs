using ApexToolsLauncher.Core.Config.GUI;
using ApexToolsLauncher.Core.Libraries;
using ApexToolsLauncher.GUI.Services;
using ApexToolsLauncher.GUI.Services.Game;
using ApexToolsLauncher.GUI.Services.Mod;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace ApexToolsLauncher.GUI.Components;

public partial class ModCard : MudComponentBase, IDisposable
{
    [Inject]
    protected IGameConfigService? GameConfigService { get; set; }
    
    [Inject]
    protected IProfileConfigService? ProfileConfigService { get; set; }
    
    [Inject]
    protected IModConfigService? ModConfigService { get; set; }
    
    [Parameter]
    public string GameId { get; set; } = ConstantsLibrary.InvalidString;
    
    [Parameter]
    public string ProfileId { get; set; } = ConstantsLibrary.InvalidString;
    
    [Parameter]
    public string ModId { get; set; } = ConstantsLibrary.InvalidString;

    public string SelectedVersion { get; set; } = ConstantsLibrary.InvalidString;
    public GameConfig GameConfig { get; set; } = new();
    public ProfileConfig ProfileConfig { get; set; } = new();
    public ModConfig ModConfig { get; set; } = new();

    protected void OnVersionChanged(string version)
    {
        if (ProfileConfigService is null) return;

        SelectedVersion = version;

        if (!ProfileConfig.ModConfigs.ContainsKey(ModId))
        {
            return;
        }
        
        ProfileConfig.ModConfigs[ModId] = version;
        ProfileConfigService.Save(GameId, ProfileId, ProfileConfig);
    }
    
    protected void OnModToggled(bool isEnabled)
    {
        if (ProfileConfigService is null) return;
        
        if (isEnabled)
        {
            TrySelectFirstVersion();
            ProfileConfig.ModConfigs.Add(ModId, SelectedVersion);
        }
        else
        {
            ProfileConfig.ModConfigs.Remove(ModId);
        }
        
        ProfileConfigService.Save(GameId, ProfileId, ProfileConfig);
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
        if (GameConfigService is null || ProfileConfigService is null || ModConfigService is null)
        {
            return;
        }
        
        GameConfig = GameConfigService.Get(GameId);
        ProfileConfig = ProfileConfigService.Get(GameId, ProfileId);
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
        ProfileConfigService?.RegisterOnReload(OnConfigReloaded);
        ModConfigService?.RegisterOnReload(OnConfigReloaded);
    }
    
    public void Dispose()
    {
        GameConfigService?.UnregisterOnReload(OnConfigReloaded);
        ProfileConfigService?.UnregisterOnReload(OnConfigReloaded);
        ModConfigService?.UnregisterOnReload(OnConfigReloaded);
    }
}