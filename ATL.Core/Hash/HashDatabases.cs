using ATL.Core.Config;
using ATL.Core.Libraries;
using RustyOptions;

namespace ATL.Core.Hash;

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
        
        Databases.Add(database);
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
        foreach (var database in Databases)
        {
            var success = database.LoadAll();
            if (!success)
                ConsoleLibrary.Log($"Failed to load database '{database.DatabaseName}'", ConsoleColor.Red);
        }
    }

    public static IEnumerable<string> AddToDatabase(string[] values, string databaseName, EHashType hashType)
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
    
    #endregion
    
    # region Hash
    
    public static bool IsKnown(uint hash, string databaseName = "")
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
    
    public static bool IsUnknown(uint hash, string databaseName = "")
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

    public static HashLookupResult Lookup(uint hash, EHashType hashType = EHashType.Unknown, string databaseName = "")
    {
        var result = new HashLookupResult
        {
            Table = hashType.ToString(),
            Database = databaseName
        };
        if (!CoreConfig.AppConfig.Cli.LookupHash)
            return result;

        if (Databases.Count == 0 && !TriedFindAndOpenAll)
        {
            FindAndOpenAll();

            if (Databases.Count == 0)
            {
                ConsoleLibrary.Log($"Failed to load any database at '{CoreConfig.AppConfig.DatabasesDirectory}'", ConsoleColor.Yellow);
                return result;
            }
        }
        
        if (string.IsNullOrEmpty(databaseName))
        {
            var optionResult = Databases
                .Select(db => db.Lookup(hash, hashType))
                .Where(r => !string.IsNullOrEmpty(r.Value))
                .FirstOrNone();

            if (optionResult.IsSome(out var safeResult))
                return safeResult;

            return result;
        }
        
        var optionDatabase = Databases
            .Where(db => string.Equals(db.DatabaseName, result.Database))
            .FirstOrNone();
        if (!optionDatabase.IsSome(out var database))
        {
            ConsoleLibrary.Log($"Failed to add '{hash}' to '{result.Database}'", ConsoleColor.Yellow);
            return result;
        }
        
        return database.Lookup(hash, hashType);
    }
    
    public static void AddKnown(uint hash, HashLookupResult result)
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