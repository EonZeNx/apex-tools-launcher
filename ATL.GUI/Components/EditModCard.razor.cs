using ATL.Core.Config.GUI;
using ATL.Core.Libraries;
using ATL.GUI.Dialogs;
using ATL.GUI.Services;
using ATL.GUI.Services.Mod;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace ATL.GUI.Components;

public partial class EditModCard : ComponentBase, IDisposable
{
    [Inject]
    public IDialogService DialogService { get; set; } = new DialogService();
    
    [Inject]
    protected ModConfigService ModConfigService { get; set; } = new();
    
    [CascadingParameter]
    public string GameId { get; set; } = "GameId";
    
    [Parameter]
    public Action<string> ModSelectionChanged { get; set; } = s => { };
    
    protected Dictionary<string, ModConfig> ModConfigs { get; set; } = [];

    protected string SelectedModId { get; set; } = ConstantsLibrary.InvalidString;
    protected ModConfig MutableModConfig { get; set; } = new();

    protected void SelectedModIdChanged(string value)
    {
        SelectedModId = value;
        ResetModConfig();
        
        ModSelectionChanged(value);
    }

    protected async void CreateModConfig()
    {
        var parameters = new DialogParameters
        {
            { "GameId", GameId }
        };
        
        var dialog = await DialogService.ShowAsync<CreateModDialog>("Create mod", parameters);
        var dialogResult = await dialog.Result;

        if (dialogResult.Canceled) return;
        
        await InvokeAsync(StateHasChanged);
    }

    protected async void ResetModConfig()
    {
        if (ModConfigs.TryGetValue(SelectedModId, out var modConfig))
        {
            MutableModConfig = (ModConfig) modConfig.Clone();
        }
        
        await InvokeAsync(StateHasChanged);
    }

    protected async void SaveModConfig()
    {
        // ModConfigService.Save(GameId, SelectedModId, MutableModConfig);
        
        await InvokeAsync(StateHasChanged);
    }
    
    protected void ReloadData()
    {
        var modConfigs = ModConfigService.GetAllFromGame(GameId);
        ModConfigs = modConfigs;
    }
    
    protected override async Task OnParametersSetAsync()
    {
        await Task.Run(ReloadData);
        
        if (ModConfigs.ContainsKey(SelectedModId)) return;
        if (ModConfigs.Count == 0) return;
        SelectedModIdChanged(ModConfigs.Keys.First());
    }
    
    protected async void OnConfigReloaded()
    {
        await Task.Run(ReloadData);
        await InvokeAsync(StateHasChanged);
    }
    
    protected override void OnInitialized()
    {
        ModConfigService.RegisterConfigReload(OnConfigReloaded);
    }

    public void Dispose()
    {
        ModConfigService.UnregisterConfigReload(OnConfigReloaded);
    }
}