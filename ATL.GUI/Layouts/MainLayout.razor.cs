using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace ATL.GUI.Layouts;

public partial class MainLayout : LayoutComponentBase
{
    protected bool UsingDarkMode { get; set; } = true;

    protected void ToggleDarkMode()
    {
        UsingDarkMode = !UsingDarkMode;
        StateHasChanged();
    }
    
    protected MudTheme Theme = new();
}