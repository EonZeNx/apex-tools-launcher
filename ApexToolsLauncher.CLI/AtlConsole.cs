using System;
using System.Collections.Generic;
using ApexToolsLauncher.Core.Libraries;

namespace ApexToolsLauncher.CLI;

public enum EMainMenuSelection
{
    Unknown = -1,
    MainMenu,
    HashMenu,
    DatabaseMenu,
    Exit
}

public static class AtlConsole
{
    public static readonly Dictionary<string, EMainMenuSelection> InputToSelection = new() {
        {"m", EMainMenuSelection.MainMenu},
        {"h", EMainMenuSelection.HashMenu},
        {"d", EMainMenuSelection.DatabaseMenu},
        {"", EMainMenuSelection.Exit}
    };
    
    public static void Loop()
    {
        DisplayMainMenu();
        
        var exit = false;
        while (!exit)
        {
            var userInput = ConsoleLibrary.GetInput("Menu input: ") ?? "_";
            var selection = InputToSelection.GetValueOrDefault(userInput, EMainMenuSelection.Unknown);

            switch (selection)
            {
            case EMainMenuSelection.MainMenu:
                DisplayMainMenu();
                break;
            case EMainMenuSelection.HashMenu:
                AtlConsoleHash.Loop();
                break;
            case EMainMenuSelection.DatabaseMenu:
                AtlConsoleDatabase.Loop();
                break;
            case EMainMenuSelection.Exit:
                exit = true;
                break;
            case EMainMenuSelection.Unknown:
            default:
                ConsoleLibrary.Log($"Unknown input '{userInput}'", ConsoleColor.Yellow);
                break;
            }
        }
    }

    public static void DisplayMainMenu()
    {
        ConsoleLibrary.Log($"{ConstantsLibrary.AppTitle} Command Line Interface {ConstantsLibrary.AppVersion}", LogType.Info);
        ConsoleLibrary.Log("[m = main menu]", LogType.Info);
        ConsoleLibrary.Log("[h = hash menu, d = database menu]", LogType.Info);
        ConsoleLibrary.Log("[empty = exit]", LogType.Info);
    }
}