using WorldOrder.World;

namespace WorldOrder.Gameplay;

public sealed class BuildableDefinition
{
    public required BlockKind Kind { get; init; }
    public required string Name { get; init; }
    public int HitPoints { get; init; }
    public Dictionary<ItemId, int> Cost { get; init; } = new();
}

public sealed class CraftingRecipe
{
    public required string Name { get; init; }
    public required ItemId Result { get; init; }
    public int Count { get; init; } = 1;
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

    public static readonly CraftingRecipe[] Recipes =
    {
        new() { Name = "Bandage", Result = ItemId.Bandage, Count = 1, Cost = new Dictionary<ItemId, int> { [ItemId.Cloth] = 2 } },
        new() { Name = "Ammo Bundle", Result = ItemId.Ammo, Count = 6, Cost = new Dictionary<ItemId, int> { [ItemId.Scrap] = 3 } },
        new() { Name = "Emergency Meal", Result = ItemId.Food, Count = 1, Cost = new Dictionary<ItemId, int> { [ItemId.Water] = 1, [ItemId.Cloth] = 1 } },
        new() { Name = "Wood Wall Kit", Result = ItemId.Wood, Count = 3, Cost = new Dictionary<ItemId, int> { [ItemId.Scrap] = 1, [ItemId.Cloth] = 1 } },
        new() { Name = "Pistol Repair", Result = ItemId.Pistol, Count = 1, Cost = new Dictionary<ItemId, int> { [ItemId.Scrap] = 8, [ItemId.Ammo] = 4 } }
    };

    public static BuildableDefinition Buildable(BlockKind kind) => Buildables.First(b => b.Kind == kind);

    public static string ItemName(ItemId item) => item switch
    {
        ItemId.Wood => "Wood",
        ItemId.Scrap => "Scrap",
        ItemId.Stone => "Stone",
        ItemId.Cloth => "Cloth",
        ItemId.Food => "Food",
        ItemId.Water => "Water",
        ItemId.Bandage => "Bandage",
        ItemId.Ammo => "Ammo",
        ItemId.Pistol => "Pistol",
        _ => item.ToString()
    };
}
