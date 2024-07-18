using ATL.Core.Config.GUI;
using ATL.Core.Libraries;
using ATL.GUI.Services;
using Microsoft.AspNetCore.Components;

namespace ATL.GUI.Components;

public partial class ModCard : ComponentBase, IDisposable
{
    [Inject]
    protected GameConfigService GameConfigService { get; set; } = new();
    
    [Inject]
    protected ProfileConfigService ProfileConfigService { get; set; } = new();
    
    [Inject]
    protected ModConfigService ModConfigService { get; set; } = new();
    
    [CascadingParameter]
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
        ProfileConfigService.Save(GameId, GameConfig.SelectedProfile, ProfileConfig);
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
        
        ProfileConfigService.Save(GameId, GameConfig.SelectedProfile, ProfileConfig);
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
        GameConfig = GameConfigService.Get(GameId);
        ProfileConfig = ProfileConfigService.Get(GameId, GameConfig.SelectedProfile);
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
        GameConfigService.RegisterConfigReload(OnConfigReloaded);
        ProfileConfigService.RegisterConfigReload(OnConfigReloaded);
        ModConfigService.RegisterConfigReload(OnConfigReloaded);
    }
    
    public void Dispose()
    {
        GameConfigService.UnregisterConfigReload(OnConfigReloaded);
        ProfileConfigService.UnregisterConfigReload(OnConfigReloaded);
        ModConfigService.UnregisterConfigReload(OnConfigReloaded);
    }
}