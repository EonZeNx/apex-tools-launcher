﻿using ATL.GUI.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace ATL.GUI.Pages;

public partial class ManagePage : MudComponentBase
{
    [Inject]
    protected AppStateService AppStateService { get; set; } = new();
    
    [Inject]
    protected LogService LogService { get; set; } = new();

    protected override Task OnParametersSetAsync()
    {
        AppStateService.SetLatestPage();
        
        return base.OnParametersSetAsync();
    }

    protected void OnGameCardClicked(string gameId)
    {
        LogService.Log($"{gameId} clicked");
    }
}
