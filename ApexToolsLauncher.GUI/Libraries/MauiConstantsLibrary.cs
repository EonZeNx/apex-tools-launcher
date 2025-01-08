
using MudBlazor;

namespace ApexToolsLauncher.GUI.Libraries;

public struct PageInfo
{
    public string Title { get; set; }
    public string Icon { get; set; }

    public PageInfo(string title, string icon)
    {
        Title = title;
        Icon = icon;
    }
}

public static class MauiConstantsLibrary
{
    public static string IdMask { get; set; } = @"^[\w ]+$";

    public static Dictionary<string, PageInfo> PageInfos { get; set; } = new()
    {
        {"manage", new PageInfo("Manage", Icons.Material.Filled.EditNote)},
        {"create", new PageInfo("Create", Icons.Material.Filled.Add)},
        {"io_tools", new PageInfo("IO tools", Icons.Material.Filled.JoinLeft)},
        {"euler", new PageInfo("Euler", Icons.Material.Filled.Rotate90DegreesCcw)},
    };
}