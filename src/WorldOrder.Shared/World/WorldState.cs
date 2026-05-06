using Microsoft.Xna.Framework;
using WorldOrder.Gameplay;

namespace WorldOrder.World;

public enum BlockKind
{
    WoodenWall,
    ReinforcedWall,
    Floor,
    Campfire
}

public sealed class PlacedBlock
{
    public required string Key { get; init; }
    public BlockKind Kind { get; set; }
    public int HitPoints { get; set; }
}

public sealed class WorldState
{
    public int Version { get; set; } = Core.Balance.SaveVersion;
    public string WorldId { get; set; } = Guid.NewGuid().ToString("N");
    public string WorldName { get; set; } = "World";
    public int Seed { get; set; }
    public float WorldTimeSeconds { get; set; } = 300f;
    public int Day { get; set; } = 1;
    public Vector2 PlayerPosition { get; set; } = new(0f, 0f);
    public PlayerVitals Vitals { get; set; } = new();
    public Inventory Inventory { get; set; } = Inventory.CreateStarter();
    public Dictionary<string, PlacedBlock> PlacedBlocks { get; set; } = new(StringComparer.Ordinal);
    public HashSet<string> DepletedNodes { get; set; } = new(StringComparer.Ordinal);
    public DateTimeOffset LastSavedUtc { get; set; } = DateTimeOffset.UtcNow;
}

public sealed class PlayerVitals
{
    public float Health { get; set; } = 100f;
    public float Hunger { get; set; } = 84f;
    public float Thirst { get; set; } = 82f;
    public float Stamina { get; set; } = 100f;
    public float Infection { get; set; }
}
