using ApexToolsLauncher.Core.Libraries;
using Microsoft.AspNetCore.Components;

namespace ApexToolsLauncher.GUI.Components;

public partial class GameCard : ComponentBase
{
    [Parameter]
    public string GameId { get; set; } = ConstantsLibrary.InvalidString;
    
    [Parameter]
    public bool Selected { get; set; } = false;

    [Parameter] public Action<string> OnClick { get; set; } = s => { };

    protected void CardClicked() => OnClick(GameId);
}