using ATL.Core.Config;
using ATL.GUI.Services;
using ATL.GUI.Services.App;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace ATL.GUI.Dialogs;

public partial class SettingsDialog : ComponentBase
{
    [Inject]
    protected AppConfigService? AppConfigService { get; set; }
    
    [CascadingParameter]
    public MudDialogInstance? MudDialog { get; set; }

    public AppConfig AppConfig { get; set; } = new();
    protected bool ValidSteamLocation { get; set; } = true;

    protected void SteamLocationChanged(string value)
    {
        AppConfig.SteamPath = value;

        if (string.IsNullOrEmpty(value))
        {
            ValidSteamLocation = false;
            return;
        }
        
        ValidSteamLocation = Directory.Exists(value);
    }
    
    protected void Close() => MudDialog?.Close(DialogResult.Cancel());

    protected void Confirm()
    {
        AppConfigService?.Save(AppConfig);
        MudDialog?.Close();
    }

    protected override void OnParametersSet()
    {
        if (AppConfigService is null)
        {
            return;
        }
        
        AppConfig = AppConfigService.Get();
    }
}
