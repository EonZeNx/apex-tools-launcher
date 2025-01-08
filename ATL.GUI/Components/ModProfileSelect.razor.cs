using ATL.Core.Config.GUI;
using ATL.Core.Libraries;
using ATL.GUI.Dialogs;
using ATL.GUI.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace ATL.GUI.Components;

public partial class ModProfileSelect : ComponentBase
{
    protected string SelectedProfile { get; set; } = "one";
}