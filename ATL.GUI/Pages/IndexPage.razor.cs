using ATL.GUI.Services;
using Microsoft.AspNetCore.Components;

namespace ATL.GUI.Pages;

public partial class IndexPage : ComponentBase
{
    [Inject]
    protected AppStateService AppStateService { get; set; } = new();

    protected override Task OnParametersSetAsync()
    {
        AppStateService.GoToLatestPage();
        
        return Task.CompletedTask;
    }
}
