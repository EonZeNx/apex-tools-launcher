using MudBlazor;

namespace ATL.GUI.Components;

public partial class ApexSwitch<T> : MudSwitch<T>
{
    private string _elementId = "switch_" + Guid.NewGuid().ToString().Substring(0, 8);
}