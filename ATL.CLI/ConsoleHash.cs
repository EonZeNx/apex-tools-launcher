using System;
using System.Globalization;
using ATL.Core.Extensions;
using ATL.Core.Hash;
using ATL.Core.Libraries;

namespace ATL.CLI;

public enum ECommand
{
    Hash,
    Lookup,
    Exit
}

public static class ConsoleHash
{
    private static string SwitchMode => "-";
    
    public static void Start()
    {
        ConsoleLibrary.Log("[- to switch between Hash and Lookup]", LogType.Info);
        ConsoleLibrary.Log("[Leave empty to exit]", LogType.Info);

        var userCommand = ECommand.Hash;
        while (userCommand != ECommand.Exit)
        {
            userCommand = userCommand switch
            {
                ECommand.Hash => HashInput(),
                ECommand.Lookup => LookupInput(),
                _ => ECommand.Exit
            };
        }
    }

    private static ECommand HashInput()
    {
        var userInput = ConsoleLibrary.GetInput("Enter string to hash: ");
        
        if (string.IsNullOrEmpty(userInput)) return ECommand.Exit;
        if (string.Equals(userInput, SwitchMode)) return ECommand.Lookup;

        var hash = userInput.HashJenkins();
        ConsoleLibrary.Log($"Hex (big endian):    {hash:X8}", ConsoleColor.White);
        ConsoleLibrary.Log($"Hex (little endian): {hash.ReverseEndian():X8}", ConsoleColor.White);
        ConsoleLibrary.Log($"UInt32:              {hash}", ConsoleColor.White);

        return ECommand.Hash;
    }

    private static ECommand LookupInput()
    {
        var userInput = ConsoleLibrary.GetInput("Enter hash to lookup: ");
        
        if (string.IsNullOrEmpty(userInput)) return ECommand.Exit;
        if (string.Equals(userInput, SwitchMode)) return ECommand.Hash;

        var parseSuccess = uint.TryParse(userInput, out var hash);
        
        if (!parseSuccess)
        {
            parseSuccess = uint.TryParse(userInput, NumberStyles.HexNumber, null, out hash);
        }
        
        if (!parseSuccess)
        {
            ConsoleLibrary.Log("Cannot hash string, is it a valid uint32?", LogType.Error);
            return ECommand.Lookup;
        }
        
        var result = LookupHashes.Get(hash);
        if (string.IsNullOrEmpty(result))
        {
            ConsoleLibrary.Log("Hash not found in database", LogType.Warning);
        }
        else
        {
            ConsoleLibrary.Log($"Found result: {result}", ConsoleColor.White);
        }
        
        return ECommand.Lookup;
    }
}