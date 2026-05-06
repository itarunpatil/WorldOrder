using WorldOrder.World;

namespace WorldOrder.Gameplay;

public sealed class BuildableDefinition
{
    public required BlockKind Kind { get; init; }
    public required string Name { get; init; }
    public int HitPoints { get; init; }
    public Dictionary<ItemId, int> Cost { get; init; } = new();
}

public static class GameDefinitions
{
    public static readonly BuildableDefinition[] Buildables =
    {
        new() { Kind = BlockKind.WoodenWall, Name = "Wood Wall", HitPoints = 120, Cost = new Dictionary<ItemId, int> { [ItemId.Wood] = 4 } },
        new() { Kind = BlockKind.ReinforcedWall, Name = "Reinforced", HitPoints = 260, Cost = new Dictionary<ItemId, int> { [ItemId.Wood] = 3, [ItemId.Scrap] = 4 } },
        new() { Kind = BlockKind.Floor, Name = "Floor", HitPoints = 60, Cost = new Dictionary<ItemId, int> { [ItemId.Wood] = 2 } },
        new() { Kind = BlockKind.Campfire, Name = "Campfire", HitPoints = 80, Cost = new Dictionary<ItemId, int> { [ItemId.Wood] = 3, [ItemId.Scrap] = 1 } }
    };

    public static BuildableDefinition Buildable(BlockKind kind) => Buildables.First(b => b.Kind == kind);
}
