using ATL.GUI.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace ATL.GUI.Layouts;

public partial class MainLayout : LayoutComponentBase
{
    [Inject]
    protected GameConfigService GameConfigService { get; set; } = new();

    [Inject]
    protected ProfileConfigService ProfileConfigService { get; set; } = new();
    
    [Inject]
    protected ModConfigService ModConfigService { get; set; } = new();

    [Inject]
    public ISnackbar? SnackbarService { get; set; }

    protected bool UsingDarkMode { get; set; } = true;

    protected void ToggleDarkMode()
    {
        UsingDarkMode = !UsingDarkMode;
        StateHasChanged();
    }
    
    protected MudTheme Theme = new();

    protected void ReloadData()
    {
        var gameConfigs = GameConfigService.GetAll();
        if (gameConfigs.Count == 0)
        {
            gameConfigs = GameConfigService.LoadAll();
        }

        foreach (var gameId in gameConfigs.Keys)
        {
            var profileConfigs = ProfileConfigService.GetGame(gameId);
            if (profileConfigs.Count == 0)
            {
                ProfileConfigService.LoadGame(gameId);
            }

            var modConfigs = ModConfigService.GetGame(gameId);
            if (modConfigs.Count == 0)
            {
                ModConfigService.LoadGame(gameId);
            }
        }
    }

    protected override async Task OnParametersSetAsync()
    {
        await Task.Run(ReloadData);
    }
}