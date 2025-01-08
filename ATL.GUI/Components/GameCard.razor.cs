using Microsoft.AspNetCore.Components;

namespace ATL.GUI.Components;

public partial class GameCard : ComponentBase
{
    [Parameter]
    public string GameID { get; set; } = "jc3";
    
    [Parameter]
    public bool Selected { get; set; } = false;
    
    [Parameter]
    public Action<string> OnClick { get; set; }

    protected void CardClicked() => OnClick(GameID);
}