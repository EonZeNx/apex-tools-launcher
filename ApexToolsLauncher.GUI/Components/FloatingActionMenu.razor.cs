using ApexToolsLauncher.GUI.Services;
using Microsoft.AspNetCore.Components;

namespace ApexToolsLauncher.GUI.Components;

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