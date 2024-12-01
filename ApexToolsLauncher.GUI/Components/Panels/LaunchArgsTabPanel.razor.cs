using ApexToolsLauncher.Core.Config.GUI;
using ApexToolsLauncher.Core.Libraries;
using ApexToolsLauncher.GUI.Services.Mod;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace ApexToolsLauncher.GUI.Components.Panels;

public partial class LaunchArgsTabPanel : MudComponentBase
{
    [Inject]
    protected IProfileConfigService? ProfileConfigService { get; set; }
    
    [Parameter]
    public string GameId { get; set; } = ConstantsLibrary.InvalidString;
    
    [Parameter]
    public string ProfileId { get; set; } = ConstantsLibrary.InvalidString;
    
    [Parameter]
    public string LaunchId { get; set; } = ConstantsLibrary.InvalidString;
    
    [Parameter]
    public LaunchOptionConfig LaunchOptionConfig { get; set; } = new();
    
    public ProfileConfig ProfileConfig { get; set; } = new();
    
    public void OnLaunchOptionToggle(bool isOn)
    {
        if (ProfileConfigService is null) return;
        
        if (isOn)
        {
            ProfileConfig.LaunchArguments.Add(LaunchId, new Dictionary<string, string>());
        }
        else
        {
            ProfileConfig.LaunchArguments.Remove(LaunchId);
        }
        
        ProfileConfigService.Save(GameId, ProfileId, ProfileConfig);
    }
    
    public void LaunchArgumentChanged(string key, string value)
    {
        if (ProfileConfigService is null) return;
        
        if (!ProfileConfig.LaunchArguments.TryGetValue(LaunchId, out var launchConfig))
        {
            return;
        }

        if (!launchConfig.TryAdd(key, value))
        {
            launchConfig[key] = value;
        }

        ProfileConfig.LaunchArguments[LaunchId] = launchConfig;
        ProfileConfigService.Save(GameId, ProfileId, ProfileConfig);
    }
    
    protected void ReloadData()
    {
        if (ProfileConfigService is null) return;
        
        ProfileConfig = ProfileConfigService.Get(GameId, ProfileId);
    }
    
    protected override async Task OnParametersSetAsync()
    {
        await Task.Run(ReloadData);
    }
}