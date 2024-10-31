using ATL.Core.Libraries;
using ATL.GUI.Services.App;
using ATL.GUI.Services.Development;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace ATL.GUI.Pages;

public partial class ManagePage : MudComponentBase
{
    [Inject]
    protected AppStateService AppStateService { get; set; } = new();
    
    [Inject]
    protected ILogService? LogService { get; set; }
    
    protected string GameId { get; set; } = "jc3";
    protected string ModId { get; set; } = ConstantsLibrary.InvalidString;

    protected void SelectedModChanged(string modId)
    {
        ModId = modId;
        StateHasChanged();
    }

    protected override Task OnParametersSetAsync()
    {
        AppStateService.SetLatestPage();
        
        return base.OnParametersSetAsync();
    }

    protected void OnGameCardClicked(string gameId)
    {
        GameId = gameId;
        LogService?.Log($"{gameId} clicked");
        StateHasChanged();
    }
}
