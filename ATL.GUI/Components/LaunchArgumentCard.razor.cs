using ATL.Core.Config.GUI;
using ATL.GUI.Services;
using Microsoft.AspNetCore.Components;

namespace ATL.GUI.Components;

public partial class LaunchArgumentCard : ComponentBase
{
    [Inject]
    protected ProfileConfigService ProfileConfigService { get; set; } = new();
    
    [CascadingParameter]
    public string GameId { get; set; } = "GameId";
    
    [Parameter]
    public string ProfileId { get; set; } = "ProfileId";
    
    [Parameter]
    public string LaunchId { get; set; } = "LaunchId";
    
    [Parameter]
    public LaunchOptionConfig LaunchOptionConfig { get; set; } = new();
    
    public ProfileConfig ProfileConfig { get; set; } = new();
    
    public void OnLaunchOptionToggle(bool isOn)
    {
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
        ProfileConfig = ProfileConfigService.Get(GameId, ProfileId);
    }
    
    protected override async Task OnParametersSetAsync()
    {
        await Task.Run(ReloadData);
    }
}