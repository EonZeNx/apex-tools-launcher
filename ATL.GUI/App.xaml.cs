using ATL.GUI.Libraries;

namespace ATL.GUI;

// ReSharper disable once RedundantExtendsListEntry
public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        MainPage = new MainPage();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var window = base.CreateWindow(activationState);

        MauiConfigLibrary.InitWindow(window);

        window.Destroying += (sender, args) => { };

        return window;
    }
}