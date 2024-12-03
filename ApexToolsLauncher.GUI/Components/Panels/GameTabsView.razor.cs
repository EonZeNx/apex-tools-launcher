using ApexToolsLauncher.Core.Libraries;
using Microsoft.AspNetCore.Components;
using Microsoft.VisualBasic;
using MudBlazor;
using Color = MudBlazor.Color;

namespace ApexToolsLauncher.GUI.Components.Panels;

public partial class GameTabsView : MudComponentBase
{
    [Parameter]
    public string GameId { get; set; } = ConstantsLibrary.InvalidString;
    
    [Parameter]
    public string ProfileId { get; set; } = ConstantsLibrary.InvalidString;
    
    protected Color BadgeColour => !ConstantsLibrary.IsStringInvalid(ProfileId) ? Color.Primary : Color.Info;
}
