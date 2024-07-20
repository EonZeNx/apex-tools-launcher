using System;
using System.Collections.Generic;
using ATL.Core.Libraries;

namespace ATL.CLI;

public enum EMainMenuSelection
{
    Unknown = -1,
    MainMenu,
    HashMenu,
    DatabaseMenu,
    Exit
}

public static class AtlCli
{
    public static readonly Dictionary<string, EMainMenuSelection> InputToSelection = new() {
        {"m", EMainMenuSelection.MainMenu},
        {"-", EMainMenuSelection.HashMenu},
        {"+", EMainMenuSelection.DatabaseMenu},
        {"", EMainMenuSelection.Exit}
    };
    
    public static void Loop()
    {
        DisplayMainMenu();
        
        var exit = false;
        while (!exit)
        {
            var userInput = ConsoleLibrary.GetInput("Select: ");
            var selection = InputToSelection.GetValueOrDefault(userInput, EMainMenuSelection.Unknown);

            switch (selection)
            {
            case EMainMenuSelection.MainMenu:
                DisplayMainMenu();
                break;
            case EMainMenuSelection.HashMenu:
                SwitchToHashMenu();
                break;
            case EMainMenuSelection.DatabaseMenu:
                break;
            case EMainMenuSelection.Exit:
                exit = true;
                break;
            case EMainMenuSelection.Unknown:
            default:
                ConsoleLibrary.Log($"unknown input '{userInput}'", ConsoleColor.Yellow);
                break;
            }
        }
    }

    public static void DisplayMainMenu()
    {
        ConsoleLibrary.Log($"{ConstantsLibrary.AppTitle} Command Line Interface {ConstantsLibrary.AppVersion}", LogType.Info);
        ConsoleLibrary.Log("[m = main menu]", LogType.Info);
        ConsoleLibrary.Log("[- = hash menu, + = database menu]", LogType.Info);
        ConsoleLibrary.Log("[empty = exit]", LogType.Info);
    }

    public static void SwitchToHashMenu()
    {
        AtlCliHash.Loop();
    }

    public static void SwitchToDatabaseMenu()
    {
        
    }
}