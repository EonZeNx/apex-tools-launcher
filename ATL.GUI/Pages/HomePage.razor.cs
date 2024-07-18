using ATL.GUI.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace ATL.GUI.Pages;

public partial class HomePage : MudComponentBase
{
    [Inject]
    protected AppStateService AppStateService { get; set; } = new();

    protected override Task OnParametersSetAsync()
    {
        AppStateService.SetLatestPage();
        
        return base.OnParametersSetAsync();
    }
}
