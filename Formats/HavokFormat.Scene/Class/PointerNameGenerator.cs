﻿namespace HavokFormat.Scene.Class;

/// <summary>
/// Generate a name based on a file position
/// </summary>
public class PointerNameGenerator
{
    public const int StartValue = 90;
    public const int AddIncrement = 1;
    
    public int Position { get; set; } = 0;
    public Dictionary<long, string> PositionNameMap = new();
    
    public PointerNameGenerator(int position)
    {
        Position = position;
    }
    
    public PointerNameGenerator() : this(StartValue)
    {  }

    public string CreateName()
    {
        var name = $"#{Position}";
        Position += AddIncrement;

        return name;
    }

    public bool Add(long position)
    {
        if (PositionNameMap.ContainsKey(position))
            return false;

        var newName = CreateName();
        PositionNameMap.TryAdd(position, newName);
        
        return true;
    }
    
    public string Get(long position)
    {
        if (PositionNameMap.TryGetValue(position, out var name))
            return name;

        return string.Empty;
    }

    public string GetOrAdd(long position)
    {
        if (PositionNameMap.TryGetValue(position, out var name))
            return name;

        var newName = CreateName();
        PositionNameMap.TryAdd(position, newName);

        return newName;
    }
}
