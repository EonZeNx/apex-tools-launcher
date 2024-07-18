using ATL.GUI.Services;
using Microsoft.AspNetCore.Components;

namespace ATL.GUI.Components;

public partial class FloatingActionMenu : ComponentBase
{
    [Inject]
    protected LaunchGameService? LaunchGameService { get; set; }
    
    [CascadingParameter]
    public string GameId { get; set; } = "GameId";

    public void LaunchGame()
    {
        LaunchGameService?.Launch(GameId);
    }
}