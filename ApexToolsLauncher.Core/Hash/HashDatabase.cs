using System.Data;
using System.Data.SQLite;
using ApexToolsLauncher.Core.Config;
using RustyOptions;

namespace ApexToolsLauncher.Core.Hash;

[Flags]
public enum EHashType
{
    FilePath = 0b_0000_0001,
    Property = 0b_0000_0010,
    Class    = 0b_0000_0100,
    Various  = 0b_0000_1000,
    
    Unknown  = 0b_1000_0000
}

public class HashLookupResult
{
    public string Value = "";
    public string Table = "unknown";
    public string Database = "unknown";

    public bool Valid() => !string.IsNullOrEmpty(Value);

    public override string ToString()
    {
        return $"'{Value}' [{Table} | {Database}]";
    }
}

public class HashDatabase
{
    public SQLiteConnection? DbConnection { get; set; } = null;
    public string DatabasePath { get; set; }
    public string DatabaseName => Path.GetFileNameWithoutExtension(DatabasePath);
    public bool ValidConnection => DbConnection?.State == ConnectionState.Open;
    
    public bool LoadedAllHashes { get; set; } = false;
    public bool TriedToOpenDb { get; set; } = false;
    
    protected readonly Dictionary<uint, HashLookupResult> KnownHashes = new();
    protected readonly HashSet<uint> UnknownHashes = new();
    
    public static Dictionary<EHashType, string> HashTypeToTable = new()
    {
        { EHashType.FilePath, "filepaths" },
        { EHashType.Property, "properties" },
        { EHashType.Class,    "classes" },
        { EHashType.Various,  "various" },
    };
    public static Dictionary<string, EHashType> TableToHashType = HashTypeToTable
        .ToDictionary(kvp => kvp.Value, kvp => kvp.Key);

    public HashDatabase(string databasePath)
    {
        DatabasePath = databasePath;
    }
    
    # region Database

    public static Option<HashDatabase> Create(string databasePath)
    {
        var result = new HashDatabase(databasePath);
        result.OpenConnection();

        if (result.ValidConnection)
            return Option.Some(result);

        return Option<HashDatabase>.None;
    }
    
    public void OpenConnection()
    {
        TriedToOpenDb = true;
        
        if (!File.Exists(DatabasePath))
            return;
            
        var dataSource = @$"Data Source={DatabasePath}";
        DbConnection = new SQLiteConnection(dataSource);
        DbConnection.Open();
    }
    
    public bool LoadAll()
    {
        if (DbConnection == null && !TriedToOpenDb)
        {
            OpenConnection();
        }

        if (DbConnection?.State != ConnectionState.Open)
            return false;
        
        using var command = DbConnection.CreateCommand();
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
        return true;
    }

    public IEnumerable<string> AddToTable(string[] values, EHashType hashType)
    {
        if (DbConnection == null && !TriedToOpenDb)
        {
            OpenConnection();
        }
        
        if (DbConnection?.State != ConnectionState.Open)
            return values;
        
        var hashTable = HashTypeToTable.GetValueOrDefault(hashType, "unknown");

        var hashResults = new Dictionary<uint, HashLookupResult>();
        foreach (var value in values)
        {
            var hash = value.HashJenkins();
            var hashResult = new HashLookupResult
            {
                Value = value,
                Table = hashTable
            };

            if (!hashResults.TryAdd(hash, hashResult))
            {
                continue;
            }
        }

        using var transaction = DbConnection.BeginTransaction();
        using var command = DbConnection.CreateCommand();
        command.Transaction = transaction;
        command.CommandType = CommandType.Text;
        // TODO: Batch in 5 - 10 at a time?
        // Ref: https://github.com/morelinq/MoreLINQ/blob/master/MoreLinq/Batch.cs
        command.CommandText = $"INSERT OR IGNORE INTO '{hashTable}' (Hash, Value)" +
                              $"VALUES (@hash, @value)";

        command.Parameters.Add(new SQLiteParameter("@hash", DbType.UInt32));
        command.Parameters.Add(new SQLiteParameter("@value", DbType.String));

        var index = 0;
        var failed = new List<string>();
        try
        {
            var hashes = hashResults.Keys.ToArray();
            for (var i = 0; i < hashes.Length; i += 1)
            {
                index = i;

                var hash = hashes[i];
                var hashResult = hashResults[hash];
                
                command.Parameters[0].Value = hash;
                command.Parameters[1].Value = hashResult.Value;

                if (command.ExecuteNonQuery() == 1)
                    continue;
                
                failed.Add(hashResult.Value);
            }
            
            transaction.Commit();
        }
        catch (Exception)
        {
            return failed.Concat(values[index..]);
        }

        return failed;
    }
    
    #endregion
    
    # region Hash

    public Option<HashLookupResult> GetKnown(uint hash)
    {
        var result = Option<HashLookupResult>.None;
        if (!IsKnown(hash))
            return result;
        
        result = KnownHashes.GetValueOrNone(hash);
        return result;
    }
    
    public bool IsKnown(uint hash)
    {
        return KnownHashes.ContainsKey(hash);
    }
    
    public bool IsUnknown(uint hash)
    {
        return UnknownHashes.Contains(hash);
    }
    
    public Option<HashLookupResult> Lookup(uint hash, EHashType hashType = EHashType.Unknown)
    {
        if (!CoreConfig.AppConfig.Cli.LookupHash)
            return Option<HashLookupResult>.None;

        if (DbConnection == null && !TriedToOpenDb)
            OpenConnection();
        if (DbConnection?.State != ConnectionState.Open)
            return Option<HashLookupResult>.None;
        
        var tables = new List<string>();
        foreach (var potentialHashType in Enum.GetValues<EHashType>())
        {
            if (potentialHashType == EHashType.Unknown) continue;
            if (hashType.HasFlag(potentialHashType) || hashType.HasFlag(EHashType.Unknown))
            {
                tables.Add(HashTypeToTable[potentialHashType]);
            }
        }
        
        var result = new HashLookupResult();
        using var command = DbConnection.CreateCommand();
        foreach (var table in tables)
        {
            command.CommandText = $"SELECT Value FROM '{table}'" +
                                  $"WHERE Hash = {(int) hash}";
            using var dbr = command.ExecuteReader();
            if (!dbr.Read())
                continue;
            
            result.Value = dbr.GetString(0);
            result.Table = table;
            result.Database = DatabaseName;
            
            break;
        }

        if (!result.Valid())
        {
            AddUnknown(hash);
            return Option<HashLookupResult>.None;
        }
        
        AddKnown(hash, result);
        return Option.Some(result);
    }
    
    public void AddKnown(uint hash, HashLookupResult result)
    {
        KnownHashes.TryAdd(hash, result);
    }
    
    public void AddUnknown(uint hash)
    {
        UnknownHashes.Add(hash);
    }
    
    #endregion
}