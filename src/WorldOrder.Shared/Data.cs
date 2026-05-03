using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace WorldOrder;

public enum Difficulty
{
    Recruit = 0,
    Veteran = 1,
    Warlord = 2
}

public enum MapPreset
{
    DesertBasin = 0,
    TwinOasis = 1,
    StonePass = 2
}

public sealed class WorldSettings
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = "New Campaign";
    public int Seed { get; set; } = 1337;
    public Difficulty Difficulty { get; set; } = Difficulty.Recruit;
    public MapPreset Preset { get; set; } = MapPreset.DesertBasin;
    public int EnemyFactions { get; set; } = 2;
    public int AllyFactions { get; set; } = 1;
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    public DateTime LastPlayedUtc { get; set; } = DateTime.UtcNow;
}

public sealed class SaveIndex
{
    public List<WorldSettings> Worlds { get; set; } = new();
}

public sealed class SaveManager
{
    private readonly string _root;
    private readonly string _indexFile;
    public SaveIndex Index { get; private set; } = new();

    public SaveManager()
    {
        var basePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        if (string.IsNullOrWhiteSpace(basePath)) basePath = AppContext.BaseDirectory;
        _root = Path.Combine(basePath, "WorldOrder");
        _indexFile = Path.Combine(_root, "worlds.json");
        Directory.CreateDirectory(_root);
        Load();
    }

    public void Load()
    {
        try
        {
            if (File.Exists(_indexFile))
            {
                Index = JsonSerializer.Deserialize<SaveIndex>(File.ReadAllText(_indexFile)) ?? new SaveIndex();
            }
        }
        catch
        {
            Index = new SaveIndex();
        }
    }

    public void Save()
    {
        Directory.CreateDirectory(_root);
        File.WriteAllText(_indexFile, JsonSerializer.Serialize(Index, new JsonSerializerOptions { WriteIndented = true }));
    }

    public WorldSettings Add(WorldSettings settings)
    {
        settings.Id = Guid.NewGuid().ToString("N");
        settings.CreatedUtc = DateTime.UtcNow;
        settings.LastPlayedUtc = DateTime.UtcNow;
        Index.Worlds.Insert(0, settings);
        Save();
        return settings;
    }

    public void Touch(WorldSettings settings)
    {
        settings.LastPlayedUtc = DateTime.UtcNow;
        Save();
    }
}
