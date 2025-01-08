using MudBlazor;

namespace ApexToolsLauncher.GUI.Components.Custom;

public partial class ApexSwitch<T> : MudSwitch<T>
{
    private string _elementId = "switch_" + Guid.NewGuid().ToString().Substring(0, 8);
}