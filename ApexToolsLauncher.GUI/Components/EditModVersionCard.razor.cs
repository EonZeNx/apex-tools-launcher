using ApexToolsLauncher.Core.Config.GUI;
using ApexToolsLauncher.Core.Libraries;
using ApexToolsLauncher.GUI.Dialogs;
using ApexToolsLauncher.GUI.Services;
using ApexToolsLauncher.GUI.Services.Mod;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace ApexToolsLauncher.GUI.Components;

public partial class EditModVersionCard : ComponentBase, IDisposable
{
    [Inject]
    public IDialogService DialogService { get; set; } = new DialogService();
    
    [Inject]
    protected ModConfigService ModConfigService { get; set; } = new();
    
    [CascadingParameter]
    public string GameId { get; set; } = "GameId";
    
    [Parameter]
    public string ModId { get; set; } = "ModId";
    
    protected Dictionary<string, ModContentConfig> ModContentConfigs { get; set; } = [];

    protected string SelectedModContentId { get; set; } = ConstantsLibrary.InvalidString;
    protected ModContentConfig MutableModContentConfig { get; set; } = new();

    protected void SelectedModContentIdChanged(string value)
    {
        SelectedModContentId = value;
        ResetModContentConfig();
    }

    protected async void CreateModContentConfig()
    {
        var parameters = new DialogParameters
        {
            { "GameId", GameId },
            { "ModId", ModId }
        };
        
        var dialog = await DialogService.ShowAsync<CreateModContentDialog>("Create mod content", parameters);
        var dialogResult = await dialog.Result;

        if (dialogResult is null) return;
        if (dialogResult.Canceled) return;
        
        await InvokeAsync(StateHasChanged);
    }
    
    protected async void ResetModContentConfig()
    {
        if (ModContentConfigs.TryGetValue(SelectedModContentId, out var modContentConfig))
        {
            MutableModContentConfig = (ModContentConfig) modContentConfig.Clone();
        }
        
        await InvokeAsync(StateHasChanged);
    }
    
    protected async void SaveModContentConfig()
    {
        var modConfig = ModConfigService.Get(GameId, ModId);
        if (modConfig.Versions.ContainsKey(SelectedModContentId))
        {
            modConfig.Versions[SelectedModContentId] = MutableModContentConfig;
        }
        
        // ModConfigService.Save(GameId, ModId, modConfig);
        
        await InvokeAsync(StateHasChanged);
    }
    
    protected void ReloadData()
    {
        var modConfig = ModConfigService.Get(GameId, ModId);
        ModContentConfigs = modConfig.Versions;
    }
    
    protected override async Task OnParametersSetAsync()
    {
        await Task.Run(ReloadData);
        
        if (ModContentConfigs.ContainsKey(SelectedModContentId)) return;
        if (ModContentConfigs.Count == 0) return;
        SelectedModContentIdChanged(ModContentConfigs.Keys.First());
    }
    
    protected async void OnConfigReloaded()
    {
        await Task.Run(ReloadData);
        await InvokeAsync(StateHasChanged);
    }
    
    protected override void OnInitialized()
    {
        ModConfigService.RegisterOnReload(OnConfigReloaded);
    }

    public void Dispose()
    {
        ModConfigService.UnregisterOnReload(OnConfigReloaded);
    }
}
