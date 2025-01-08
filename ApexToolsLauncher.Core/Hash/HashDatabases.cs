using ApexToolsLauncher.Core.Config;
using ApexToolsLauncher.Core.Libraries;
using RustyOptions;

namespace ApexToolsLauncher.Core.Hash;

public class HashDatabases
{
    public static List<HashDatabase> Databases { get; set; } = [];
    public static bool TriedFindAndOpenAll { get; set; } = false;
    
    # region Database
    
    public static void OpenConnection(string filePath)
    {
        var optionDatabase = HashDatabase.Create(filePath);
        if (!optionDatabase.IsSome(out var database))
        {
            ConsoleLibrary.Log($"Failed to open database '{filePath}'", ConsoleColor.Red);
            return;
        }

        lock (Databases)
        {
            Databases.Add(database);
        }
    }
    
    public static void FindAndOpenAll()
    {
        if (TriedFindAndOpenAll)
            return;
        
        var databaseDirectory = CoreConfig.AppConfig.DatabasesDirectory;
        
        var databasePaths = Directory.GetFiles(databaseDirectory, "*.db");
        if (databasePaths.Length == 0)
        {
            ConsoleLibrary.Log($"Failed to find any databases in '{databaseDirectory}'", ConsoleColor.Red);
            return;
        }

        foreach (var databasePath in databasePaths)
        {
            OpenConnection(databasePath);
        }

        TriedFindAndOpenAll = true;
    }
    
    public static void LoadAll()
    {
        lock (Databases)
        {
            foreach (var database in Databases)
            {
                var success = database.LoadAll();
                if (!success)
                    ConsoleLibrary.Log($"Failed to load database '{database.DatabaseName}'", ConsoleColor.Red);
            }
        }
    }

    public static IEnumerable<string> AddToDatabase(string[] values, string databaseName, EHashType hashType)
    {
        lock (Databases)
        {
            var optionDatabase = Databases
                .Where(db => string.Equals(db.DatabaseName, databaseName))
                .FirstOrNone();
            if (!optionDatabase.IsSome(out var database))
            {
                ConsoleLibrary.Log($"Failed to find database '{databaseName}'", ConsoleColor.Yellow);
                return values;
            }
        
            return database.AddToTable(values, hashType);
        }
    }
    
    #endregion
    
    # region Hash
    
    public static bool IsKnown(uint hash, string databaseName = "")
    {
        lock (Databases)
        {
            if (string.IsNullOrEmpty(databaseName))
            {
                if (Databases.Any(db => db.IsKnown(hash)))
                {
                    return true;
                }
            }

            var optionDatabase = Databases
                .Where(db => string.Equals(db.DatabaseName, databaseName))
                .FirstOrNone();
            if (!optionDatabase.IsSome(out var database))
            {
                ConsoleLibrary.Log($"Failed to find database '{databaseName}'", ConsoleColor.Yellow);
                return false;
            }
        
            return database.IsKnown(hash);
        }
    }
    
    public static bool IsUnknown(uint hash, string databaseName = "")
    {
        lock (Databases)
        {
            if (string.IsNullOrEmpty(databaseName))
            {
                if (Databases.Any(db => db.IsUnknown(hash)))
                {
                    return true;
                }
            }

            var optionDatabase = Databases
                .Where(db => string.Equals(db.DatabaseName, databaseName))
                .FirstOrNone();
            if (!optionDatabase.IsSome(out var database))
            {
                ConsoleLibrary.Log($"Failed to find database '{databaseName}'", ConsoleColor.Yellow);
                return false;
            }
        
            return database.IsUnknown(hash);
        }
    }

    public static Option<HashLookupResult> Lookup(uint hash, EHashType hashType = EHashType.Unknown, string databaseName = "")
    {
        lock (Databases)
        {
            if (!CoreConfig.AppConfig.Cli.LookupHash)
                return Option<HashLookupResult>.None;

            if (Databases.Count == 0 && !TriedFindAndOpenAll)
                FindAndOpenAll();
        
            if (Databases.Count == 0)
            {
                ConsoleLibrary.Log($"Failed to load any databases at '{CoreConfig.AppConfig.DatabasesDirectory}'", ConsoleColor.Yellow);
                return Option<HashLookupResult>.None;
            }
        
            if (string.IsNullOrEmpty(databaseName))
            {
                foreach (var db in Databases)
                {
                    var optionResult = db.GetKnown(hash);
                
                    if (optionResult.IsNone)
                        optionResult = db.Lookup(hash);

                    if (!optionResult.IsNone)
                        return optionResult;
                }

                return Option<HashLookupResult>.None;
            }
        
            var optionDatabase = Databases
                .Where(db => string.Equals(db.DatabaseName, databaseName))
                .FirstOrNone();
            if (!optionDatabase.IsSome(out var database))
            {
                ConsoleLibrary.Log($"Failed to find '{hash}' in '{databaseName}'", ConsoleColor.Yellow);
                return Option<HashLookupResult>.None;
            }
        
            return database.Lookup(hash, hashType);
        }
    }
    
    public static void AddKnown(uint hash, HashLookupResult result)
    {
        lock (Databases)
        {
            var optionDatabase = Databases
                .Where(db => string.Equals(db.DatabaseName, result.Database))
                .FirstOrNone();
            if (!optionDatabase.IsSome(out var database))
            {
                ConsoleLibrary.Log($"Failed to add '{hash}' to '{result.Database}'", ConsoleColor.Yellow);
                return;
            }
        
            database.AddKnown(hash, result);
        }
    }
    
    public static void AddUnknown(uint hash, string databaseName)
    {
        var optionDatabase = Databases
            .Where(db => string.Equals(db.DatabaseName, databaseName))
            .FirstOrNone();
        if (!optionDatabase.IsSome(out var database))
        {
            ConsoleLibrary.Log($"Failed to add '{hash}' to '{databaseName}'", ConsoleColor.Yellow);
            return;
        }
        
        database.AddUnknown(hash);
    }
    
    #endregion
}