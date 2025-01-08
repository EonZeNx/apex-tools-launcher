using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ATL.Core.Config;
using ATL.Core.Hash;
using ATL.Core.Libraries;
using RustyOptions;

namespace ATL.CLI;

public enum EDatabaseSelection
{
    Unknown = -1,
    Add,
    Exit
}

public static class AtlConsoleDatabase
{
    public static readonly Dictionary<string, EDatabaseSelection> InputToSelection = new() {
        {"+", EDatabaseSelection.Add},
        {"", EDatabaseSelection.Exit}
    };
    
    public static void Loop()
    {
        DisplayAddMenu();
        
        var exit = false;
        while (!exit)
        {
            var userInput = ConsoleLibrary.GetInput("Database input: ") ?? "_";
            var selection = InputToSelection.GetValueOrDefault(userInput, EDatabaseSelection.Unknown);
            
            switch (selection)
            {
                case EDatabaseSelection.Add:
                    AddToDatabase();
                    break;
                case EDatabaseSelection.Exit:
                    exit = true;
                    break;
                case EDatabaseSelection.Unknown:
                default:
                    ConsoleLibrary.Log($"Unknown input '{userInput}'", ConsoleColor.Yellow);
                    break;
            }
        }
    }

    public static void DisplayAddMenu()
    {
        ConsoleLibrary.Log("[+ = add to database]", LogType.Info);
        ConsoleLibrary.Log("[empty = exit]", LogType.Info);
    }

    public static void AddToDatabase()
    {
        ConsoleLibrary.Log("[Add to Database]", LogType.Info);

        var optionFilePath = GetValueFile();
        if (!optionFilePath.IsSome(out var filePath))
            return;

        var optionHashType = GetHashType();
        if (!optionHashType.IsSome(out var hashType))
            return;
        if (hashType == EHashType.Unknown)
            return;

        var optionDatabaseName = GetDatabaseName();
        if (!optionDatabaseName.IsSome(out var databaseName))
            return;
        if (string.IsNullOrEmpty(databaseName))
            return;

        var dateTimeString = DateTime.Now.ToString("yyyy-MMM-dd_hh-mm-ss").ToUpper();
        var databasePath = Path.Join(CoreConfig.AppConfig.DatabasesDirectory, $"{databaseName}.db");
        if (File.Exists(databasePath))
        {
            var databaseDirectoryPath = Path.GetDirectoryName(databasePath);
            if (string.IsNullOrEmpty(databaseDirectoryPath))
            {
                ConsoleLibrary.Log($"Failed to locate database", ConsoleColor.Yellow);
                return;
            }
            
            var databaseBackupPath = Path.Join(databaseDirectoryPath, $"{databaseName}_{dateTimeString}.db");
            File.Copy(databasePath, databaseBackupPath);
            ConsoleLibrary.Log($"Database backup created at '{databaseBackupPath}'", ConsoleColor.White);
        }
        
        var lines = File.ReadLines(filePath).ToArray();
        var failed = HashDatabases.AddToDatabase(lines, databaseName, hashType).ToArray();

        if (failed.Length > 0)
        {
            ConsoleLibrary.Log($"Failed to add {failed.Length} values", ConsoleColor.Yellow);
            
            try
            { // Save failed values to file
                var logPath = Path.Join(ConfigLibrary.GetBasePath(), CoreConfig.AppConfig.LogPath);
                if (!Directory.Exists(logPath))
                    Directory.CreateDirectory(logPath);
                
                var failedFilePath = Path.Join(logPath, $"db_add_failed_{dateTimeString}.txt");
                using var failedFileStream = new FileStream(failedFilePath, FileMode.Create);

                for (var i = 0; i < failed.Length; i += 1)
                {
                    var failedString = failed[i];
                    if (i != 0)
                        failedString = $"\n{failedString}";
                    
                    failedFileStream.Write(Encoding.UTF8.GetBytes(failedString));
                }
                
                ConsoleLibrary.Log($"Saved failed values to '{failedFilePath}'", ConsoleColor.Yellow);
            }
            catch (Exception)
            {
                ConsoleLibrary.Log("Failed to save failed values", ConsoleColor.Red);
            }
        }
    }

    public static Option<string> GetValueFile()
    {
        string filePath;
        while (true)
        {
            var userInput = ConsoleLibrary.GetInput("Target filepath: ") ?? string.Empty;

            if (string.IsNullOrEmpty(userInput))
                return Option<string>.None;

            if (!File.Exists(userInput))
            {
                ConsoleLibrary.Log($"File does not exist '{userInput}'", ConsoleColor.Yellow);
                continue;
            }

            filePath = userInput;
            break;
        }
        
        return Option.Some(filePath);
    }

    public static Option<EHashType> GetHashType()
    {
        EHashType hashType;
        
        var hashTypeMessage = "Choose a hash type";
        hashTypeMessage += string.Join("", HashDatabase.TableToHashType
            .Where(kvp => kvp.Value != EHashType.Unknown)
            .Select(kvp => $"\n- {kvp.Key}"));
        ConsoleLibrary.Log(hashTypeMessage, ConsoleColor.White);
        
        while (true)
        {
            var userInput = ConsoleLibrary.GetInput("Hash type: ") ?? string.Empty;

            if (string.IsNullOrEmpty(userInput))
                return Option<EHashType>.None;

            hashType = HashDatabase.TableToHashType.GetValueOrDefault(userInput.ToLower(), EHashType.Unknown);
            if (hashType != EHashType.Unknown)
                break;

            ConsoleLibrary.Log($"Invalid input '{userInput}'", ConsoleColor.Yellow);
            ConsoleLibrary.Log(hashTypeMessage, ConsoleColor.White);
        }
        
        return Option.Some(hashType);
    }

    public static Option<string> GetDatabaseName()
    {
        string databaseName;

        var databasePaths = Directory.GetFiles(CoreConfig.AppConfig.DatabasesDirectory, "*.db");
        var databaseNames = databasePaths.Select(Path.GetFileNameWithoutExtension).ToList();
        
        var databaseMessage = "Choose a hash database";
        databaseMessage += string.Join("", databasePaths.Select(db => $"\n- {Path.GetFileNameWithoutExtension(db)}"));
        ConsoleLibrary.Log(databaseMessage, ConsoleColor.White);
        
        while (true)
        {
            var userInput = ConsoleLibrary.GetInput("Database: ") ?? string.Empty;

            if (string.IsNullOrEmpty(userInput))
                return Option<string>.None;

            databaseName = userInput;
            if (databaseNames.Contains(databaseName))
                break;

            ConsoleLibrary.Log($"Invalid input '{userInput}'", ConsoleColor.Yellow);
            ConsoleLibrary.Log(databaseMessage, ConsoleColor.White);
        }
        
        return Option.Some(databaseName);
    }
}