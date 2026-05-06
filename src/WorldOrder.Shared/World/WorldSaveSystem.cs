using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Xna.Framework;
using WorldOrder.Core;
using WorldOrder.Gameplay;

namespace WorldOrder.World;

public sealed class WorldSaveInfo
{
    public required string WorldId { get; init; }
    public required string Name { get; init; }
    public int Seed { get; init; }
    public string MapId { get; init; } = WorldMapCatalog.DefaultMapId;
    public int Day { get; init; }
    public DateTimeOffset LastSavedUtc { get; init; }
    public required string Folder { get; init; }
}

public static class WorldSaveSystem
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        Converters = { new Vector2JsonConverter() }
    };

    public static IReadOnlyList<WorldSaveInfo> ListSaves()
    {
        Directory.CreateDirectory(StoragePaths.SaveRoot);
        var saves = new List<WorldSaveInfo>();
        foreach (var dir in Directory.EnumerateDirectories(StoragePaths.SaveRoot))
        {
            var file = Path.Combine(dir, "world.json");
            if (!File.Exists(file)) continue;
            try
            {
                var state = JsonSerializer.Deserialize<WorldState>(File.ReadAllText(file), Options);
                if (state is null) continue;
                saves.Add(new WorldSaveInfo
                {
                    WorldId = state.WorldId,
                    Name = state.WorldName,
                    Seed = state.Seed,
                    MapId = string.IsNullOrWhiteSpace(state.MapId) ? WorldMapCatalog.DefaultMapId : state.MapId,
                    Day = state.Day,
                    LastSavedUtc = state.LastSavedUtc,
                    Folder = dir
                });
            }
            catch
            {
                // Corrupt saves are ignored by the list, but not deleted.
            }
        }
        return saves.OrderByDescending(s => s.LastSavedUtc).ToList();
    }

    public static WorldState CreateNew(string name, int seed, string mapId = WorldMapCatalog.DefaultMapId)
    {
        var id = Guid.NewGuid().ToString("N");
        return new WorldState
        {
            WorldId = id,
            WorldName = string.IsNullOrWhiteSpace(name) ? "WORLD" : name.Trim(),
            Seed = seed,
            MapId = string.IsNullOrWhiteSpace(mapId) ? WorldMapCatalog.DefaultMapId : mapId,
            PlayerPosition = WorldMapCatalog.SpawnFor(mapId),
            Inventory = Inventory.CreateStarter()
        };
    }

    public static WorldState Load(string folder)
    {
        var file = Path.Combine(folder, "world.json");
        var state = JsonSerializer.Deserialize<WorldState>(File.ReadAllText(file), Options);
        if (state is null) throw new InvalidDataException("Save file did not contain a world state.");
        return state;
    }

    public static void Save(WorldState state)
    {
        state.LastSavedUtc = DateTimeOffset.UtcNow;
        var dir = Path.Combine(StoragePaths.SaveRoot, state.WorldId);
        Directory.CreateDirectory(dir);
        var temp = Path.Combine(dir, "world.json.tmp");
        var final = Path.Combine(dir, "world.json");
        File.WriteAllText(temp, JsonSerializer.Serialize(state, Options));
        if (File.Exists(final)) File.Delete(final);
        File.Move(temp, final);
    }
}

public sealed class Vector2JsonConverter : JsonConverter<Vector2>
{
    public override Vector2 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject) throw new JsonException();
        float x = 0f;
        float y = 0f;
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject) return new Vector2(x, y);
            var prop = reader.GetString();
            reader.Read();
            if (string.Equals(prop, "X", StringComparison.OrdinalIgnoreCase)) x = reader.GetSingle();
            else if (string.Equals(prop, "Y", StringComparison.OrdinalIgnoreCase)) y = reader.GetSingle();
            else reader.Skip();
        }
        throw new JsonException();
    }

    public override void Write(Utf8JsonWriter writer, Vector2 value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteNumber("X", value.X);
        writer.WriteNumber("Y", value.Y);
        writer.WriteEndObject();
    }
}
