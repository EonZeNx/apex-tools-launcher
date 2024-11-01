using ATL.Core.Libraries;

namespace ATL.GUI.Libraries;

public static class MauiConfigLibrary
{
    private static Window AppWindow { get; set; } = new();

    private static string CreateAppTitle(string extra = "")
    {
        var result = $"{ConstantsLibrary.AppFullTitle} {ConstantsLibrary.AppVersion}";
        if (extra.Length > 0)
        {
            result += $" - {extra}";
        }

        return result;
    }

    public static void InitWindow(Window window)
    {
        AppWindow = window;
        AppWindow.Title = CreateAppTitle();
        
        AppWindow.Width = ConstantsLibrary.AppWindowSize[0];
        AppWindow.Height = ConstantsLibrary.AppWindowSize[1];
    }
    
    public static void SetTitle(string title)
    {
        AppWindow.Title = CreateAppTitle(title);
    }
    
    public static void ResetTitle()
    {
        AppWindow.Title = CreateAppTitle();
    }
}