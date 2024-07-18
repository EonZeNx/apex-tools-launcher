
using MudBlazor;

namespace ATL.GUI.Libraries;

public static class MauiConstantsLibrary
{
    public static IMask IdMask { get; set; } = new RegexMask(@"^[\w ]+$");
}