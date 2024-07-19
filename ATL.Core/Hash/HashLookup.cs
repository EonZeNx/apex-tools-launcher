using System.Data;
using System.Data.SQLite;
using ATL.Core.Config;

namespace ATL.Core.Hash;

[Flags]
public enum EHashType
{
    Unknown = 0,
    FilePath = 1,
    Property = 2,
    Class = 4,
    Misc = 8
}

public class HashLookupResult
{
    public string Value = "";
    public string Table = "unknown";

    public bool Valid() => !string.IsNullOrEmpty(Value);
}

public static class LookupHashes
{
    public static SQLiteConnection? DbConnection { get; set; } = null;
    public static bool TriedToOpenDb { get; set; } = false;
    public static bool LoadedAllHashes { get; set; } = false;
    
    public static readonly Dictionary<uint, HashLookupResult> KnownHashes = new();
    public static readonly HashSet<uint> UnknownHashes = new();
    
    public static readonly Dictionary<EHashType, string> HashTypeToTable = new()
    {
        { EHashType.FilePath, "filepaths" },
        { EHashType.Property, "properties" },
        { EHashType.Class, "classes" },
        { EHashType.Misc, "misc" },
    };
    
    # region Database
    
    public static void OpenDatabaseConnection()
    {
        TriedToOpenDb = true;
        
        var dbFile = CoreAppConfig.Get().Cli.DatabasePath;
        if (!File.Exists(dbFile)) return;
            
        var dataSource = @$"Data Source={dbFile}";
        DbConnection = new SQLiteConnection(dataSource);
        DbConnection.Open();
    }
    
    public static void LoadAll()
    {
        if (DbConnection == null && !TriedToOpenDb)
        {
            OpenDatabaseConnection();
        }
        
        if (DbConnection?.State != ConnectionState.Open) return;
        
        var command = DbConnection.CreateCommand();
        var tables = new List<string>(HashTypeToTable.Values);
        
        foreach (var table in tables)
        {
            command.CommandText = $"SELECT Hash, Value FROM '{table}'";
            using var dbr = command.ExecuteReader();
            while (dbr.Read())
            {
                var hash = (uint) dbr.GetInt32(0);
                var result = new HashLookupResult
                {
                    Value = dbr.GetString(1),
                    Table = table
                };

                KnownHashes.TryAdd(hash, result);
            }
        }

        LoadedAllHashes = true;
    }
    
    #endregion
    
    # region Hash
    
    public static bool Known(uint hash)
    {
        return KnownHashes.ContainsKey(hash);
    }
    
    public static bool Unknown(uint hash)
    {
        return UnknownHashes.Contains(hash);
    }
    
    public static HashLookupResult Get(byte[] bytes, EHashType hashType = EHashType.Unknown)
    {
        return Get(BitConverter.ToUInt32(bytes), hashType);
    }
    
    public static HashLookupResult Get(uint hash, EHashType hashType = EHashType.Unknown)
    {
        var result = new HashLookupResult();
        if (!CoreAppConfig.Get().Cli.LookupHash) return result;

        if (Known(hash))
        {
            result = KnownHashes[hash];
            return result;
        }
        if (Unknown(hash)) return result;
        if (LoadedAllHashes)
        { // don't bother searching
            AddUnknown(hash);
            return result;
        }

        if (DbConnection == null && !TriedToOpenDb) OpenDatabaseConnection();
        if (DbConnection?.State != ConnectionState.Open) return result;
        
        var tables = new List<string>();
        if (hashType.HasFlag(EHashType.FilePath) || hashType.HasFlag(EHashType.Unknown))
        {
            tables.Add(HashTypeToTable[EHashType.FilePath]);
        }
        if (hashType.HasFlag(EHashType.Property) || hashType.HasFlag(EHashType.Unknown))
        {
            tables.Add(HashTypeToTable[EHashType.Property]);
        }
        if (hashType.HasFlag(EHashType.Class) || hashType.HasFlag(EHashType.Unknown))
        {
            tables.Add(HashTypeToTable[EHashType.Class]);
        }
        if (hashType.HasFlag(EHashType.Misc) || hashType.HasFlag(EHashType.Unknown))
        {
            tables.Add(HashTypeToTable[EHashType.Misc]);
        }
        
        var command = DbConnection.CreateCommand();
        foreach (var table in tables)
        {
            command.CommandText = $"SELECT Value FROM '{table}' WHERE Hash = {(int) hash}";
            using var dbr = command.ExecuteReader();
            if (!dbr.Read()) continue;
            
            result.Value = dbr.GetString(0);
            result.Table = table;
            break;
        }

        if (!string.IsNullOrEmpty(result.Value))
        {
            AddKnown(hash, result);
        }
        else
        {
            AddUnknown(hash);
        }
        
        return result;
    }
    
    public static void AddKnown(uint hash, HashLookupResult result)
    {
        KnownHashes.TryAdd(hash, result);
    }
    
    public static void AddUnknown(uint hash)
    {
        UnknownHashes.Add(hash);
    }
    
    #endregion
}