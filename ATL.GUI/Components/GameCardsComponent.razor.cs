using Microsoft.AspNetCore.Components;

namespace ATL.GUI.Components;

public partial class GameCardsComponent : ComponentBase
{
    [Parameter]
    public string SelectedGameId { get; set; } = "jc3";
    
    [Parameter]
    public Action<string> SelectedGameChanged { get; set; }

    protected void OnGameCardClicked(string gameId)
    {
        SelectedGameId = gameId;
        SelectedGameChanged(gameId);
        StateHasChanged();
    }

    protected List<string> TestGameIds = ["jc3", "jc4", "jc5", "jc6"];
}