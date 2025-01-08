
using MudBlazor;

namespace ApexToolsLauncher.GUI.Libraries;

public struct AltPageInfo
{
    public string Title { get; set; }
    public string Icon { get; set; }

    public AltPageInfo(string title, string icon)
    {
        Title = title;
        Icon = icon;
    }
}

public static class MauiConstantsLibrary
{
    public static IMask IdMask { get; set; } = new RegexMask(@"^[\w ]+$");

    public static Dictionary<string, AltPageInfo> PageInfos { get; set; } = new()
    {
        {"manage", new AltPageInfo("Manage", Icons.Material.Filled.EditNote)},
        {"create", new AltPageInfo("Create", Icons.Material.Filled.Add)},
        {"io_tools", new AltPageInfo("IO tools", Icons.Material.Filled.JoinLeft)},
        {"euler", new AltPageInfo("Euler", Icons.Material.Filled.Rotate90DegreesCcw)},
    };
}