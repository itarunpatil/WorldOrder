using WorldOrder.Core;
using WorldOrder.World;

namespace WorldOrder.Gameplay;

public static class LootTables
{
    public static IEnumerable<(ItemId Item, int Count)> FromResource(ResourceNode node, int seed)
    {
        var a = 1 + Hashing.Range(node.Id.Length, node.MaxDurability, seed, 0, 2);
        return node.Kind switch
        {
            ResourceKind.Tree => new[] { (ItemId.Wood, 2 + a), (ItemId.Cloth, Hashing.Range(node.MaxDurability, 3, seed, 0, 2)) },
            ResourceKind.ScrapPile => new[] { (ItemId.Scrap, 2 + a), (ItemId.Cloth, 1) },
            ResourceKind.WreckedCar => new[] { (ItemId.Scrap, 4 + a), (ItemId.Ammo, Hashing.Range(node.MaxDurability, 9, seed, 1, 4)) },
            ResourceKind.Barrel => new[] { (ItemId.Scrap, 1 + a), (ItemId.Water, Hashing.Range(node.MaxDurability, 11, seed, 0, 2)) },
            ResourceKind.FoodCache => new[] { (ItemId.Food, 2), (ItemId.Water, 1) },
            ResourceKind.WaterCache => new[] { (ItemId.Water, 2) },
            ResourceKind.MedicalCache => new[] { (ItemId.Bandage, 1), (ItemId.Cloth, 2) },
            ResourceKind.AmmoCache => new[] { (ItemId.Ammo, 6), (ItemId.Pistol, 1) },
            _ => Array.Empty<(ItemId, int)>()
        };
    }
}
