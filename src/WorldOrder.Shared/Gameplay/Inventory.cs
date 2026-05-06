namespace WorldOrder.Gameplay;

public enum ItemId
{
    Wood,
    Scrap,
    Stone,
    Cloth,
    Food,
    Water,
    Bandage,
    Ammo,
    Pistol
}

public sealed class ItemStack
{
    public ItemId Item { get; set; }
    public int Count { get; set; }
}

public sealed class Inventory
{
    public List<ItemStack> Items { get; set; } = new();

    public static readonly ItemId[] HotbarOrder =
    {
        ItemId.Wood,
        ItemId.Scrap,
        ItemId.Food,
        ItemId.Water,
        ItemId.Bandage,
        ItemId.Ammo,
        ItemId.Pistol,
        ItemId.Cloth
    };

    public static Inventory CreateStarter()
    {
        var inventory = new Inventory();
        inventory.Add(ItemId.Food, 2);
        inventory.Add(ItemId.Water, 2);
        inventory.Add(ItemId.Wood, 6);
        inventory.Add(ItemId.Scrap, 2);
        inventory.Add(ItemId.Cloth, 1);
        return inventory;
    }

    public int Count(ItemId item) => Items.FirstOrDefault(i => i.Item == item)?.Count ?? 0;

    public void Add(ItemId item, int count)
    {
        if (count <= 0) return;
        var stack = Items.FirstOrDefault(i => i.Item == item);
        if (stack is null)
        {
            Items.Add(new ItemStack { Item = item, Count = count });
        }
        else
        {
            stack.Count += count;
        }
    }

    public bool Remove(ItemId item, int count)
    {
        if (count <= 0) return true;
        var stack = Items.FirstOrDefault(i => i.Item == item);
        if (stack is null || stack.Count < count) return false;
        stack.Count -= count;
        if (stack.Count <= 0) Items.Remove(stack);
        return true;
    }

    public bool HasCost(IReadOnlyDictionary<ItemId, int> cost)
    {
        foreach (var pair in cost)
        {
            if (Count(pair.Key) < pair.Value) return false;
        }
        return true;
    }

    public bool Pay(IReadOnlyDictionary<ItemId, int> cost)
    {
        if (!HasCost(cost)) return false;
        foreach (var pair in cost) Remove(pair.Key, pair.Value);
        return true;
    }
}
