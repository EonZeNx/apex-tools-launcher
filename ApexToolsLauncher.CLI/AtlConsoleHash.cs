using System;
using System.Collections.Generic;
using System.Globalization;
using ApexToolsLauncher.Core.Extensions;
using ApexToolsLauncher.Core.Hash;
using ApexToolsLauncher.Core.Libraries;

namespace ApexToolsLauncher.CLI;

public enum EHashMenuSelection
{
    Action,
    Hash,
    Lookup,
    Exit
}

public static class AtlConsoleHash
{
    public static readonly Dictionary<string, EHashMenuSelection> InputToSelection = new() {
        {"-", EHashMenuSelection.Hash},
        {"+", EHashMenuSelection.Lookup},
        {"", EHashMenuSelection.Exit}
    };
    
    public static void Loop()
    {
        DisplayHashMenu();
        
        var selection = EHashMenuSelection.Hash;
        while (true)
        {
            var inputMessage = selection switch
            {
                EHashMenuSelection.Hash => "Hash input",
                EHashMenuSelection.Lookup => "Lookup input",
                _ => "Input"
            };

            var userInput = ConsoleLibrary.GetInput($"{inputMessage}: ") ?? "";
            var newSelection = InputToSelection.GetValueOrDefault(userInput, EHashMenuSelection.Action);

            if (newSelection == EHashMenuSelection.Exit) break;

            if (newSelection == EHashMenuSelection.Action)
            {
                switch (selection)
                {
                case EHashMenuSelection.Hash:
                    HashInput(userInput);
                    continue;
                case EHashMenuSelection.Lookup:
                    LookupInput(userInput);
                    continue;
                default:
                    ConsoleLibrary.Log($"Unknown action {selection}", LogType.Warning);
                    continue;
                }
            }
            
            selection = newSelection;
        }
    }

    public static void DisplayHashMenu()
    {
        ConsoleLibrary.Log("[- = hash, + = Lookup]", LogType.Info);
        ConsoleLibrary.Log("[empty = exit]", LogType.Info);
    }

    private static void HashInput(string input)
    {
        var hash = input.HashJenkins();
        ConsoleLibrary.Log($"Hex (big):    {hash:X8}", ConsoleColor.White);
        ConsoleLibrary.Log($"Hex (little): {hash.ReverseEndian():X8}", ConsoleColor.White);
        ConsoleLibrary.Log($"uint32:       {hash}", ConsoleColor.White);
        ConsoleLibrary.Log($"int32:        {(int) hash}", ConsoleColor.White);
    }

    private static void LookupInput(string input)
    {
        var parseSuccess = uint.TryParse(input, out var hash);
        if (!parseSuccess)
        {
            parseSuccess = uint.TryParse(input, NumberStyles.HexNumber, null, out hash);
        }
        
        if (!parseSuccess)
        {
            ConsoleLibrary.Log("Cannot hash string, is it a valid uint32?", LogType.Error);
            return;
        }
        
        var optionResult = HashDatabases.Lookup(hash);
        if (optionResult.IsSome(out var result))
            ConsoleLibrary.Log($"Found result: {result.Value} [{result.Table} | {result.Database}]", ConsoleColor.White);
        else
            ConsoleLibrary.Log("Hash not found in database", LogType.Warning);
    }
}