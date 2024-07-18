using ATL.Core.Config.GUI;
using Microsoft.AspNetCore.Components;

namespace ATL.GUI.Components;

public partial class CreateModVersionCard : ComponentBase
{
    [Parameter]
    public string VersionId { get; set; } = "ModVersionId";
    
    [Parameter]
    public ModContentConfig ContentConfig { get; set; } = new();

    protected bool Expanded { get; set; } = true;

    protected void ToggleExpand() => Expanded = !Expanded;
}