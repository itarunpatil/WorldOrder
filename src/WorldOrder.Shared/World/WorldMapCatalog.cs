using Microsoft.Xna.Framework;
using WorldOrder.Core;

namespace WorldOrder.World;

public sealed class WorldMapDefinition
{
    public WorldMapDefinition(string id, string name, string description, int width, int height, Vector2 spawn)
    {
        Id = id;
        Name = name;
        Description = description;
        Width = width;
        Height = height;
        Spawn = spawn;
        Tiles = new TileType[width * height];
        Array.Fill(Tiles, TileType.DryGrass);
    }

    public string Id { get; }
    public string Name { get; }
    public string Description { get; }
    public int Width { get; }
    public int Height { get; }
    public Vector2 Spawn { get; }
    public TileType[] Tiles { get; }
    public List<MapResourcePlacement> Resources { get; } = new();
    public List<MapDecorationPlacement> Decorations { get; } = new();
    public List<Vector2> ZombieSpawnPoints { get; } = new();

    public int OriginTileX => -Width / 2;
    public int OriginTileY => -Height / 2;
    public int MinTileX => OriginTileX;
    public int MinTileY => OriginTileY;
    public int MaxTileX => OriginTileX + Width - 1;
    public int MaxTileY => OriginTileY + Height - 1;

    public bool ContainsTile(int worldTileX, int worldTileY)
    {
        var lx = worldTileX - OriginTileX;
        var ly = worldTileY - OriginTileY;
        return lx >= 0 && ly >= 0 && lx < Width && ly < Height;
    }

    public TileType TileAt(int worldTileX, int worldTileY)
    {
        var lx = worldTileX - OriginTileX;
        var ly = worldTileY - OriginTileY;
        if (lx < 0 || ly < 0 || lx >= Width || ly >= Height) return TileType.Rubble;
        return Tiles[ly * Width + lx];
    }

    public void SetTile(int worldTileX, int worldTileY, TileType tile)
    {
        var lx = worldTileX - OriginTileX;
        var ly = worldTileY - OriginTileY;
        if (lx < 0 || ly < 0 || lx >= Width || ly >= Height) return;
        Tiles[ly * Width + lx] = tile;
    }

    public void PaintRect(int x0, int y0, int width, int height, TileType tile)
    {
        for (var y = y0; y < y0 + height; y++)
            for (var x = x0; x < x0 + width; x++)
                SetTile(x, y, tile);
    }

    public void PaintRoadHorizontal(int centerY, int x0, int x1, int roadHalfWidth = 3, int sidewalkWidth = 2)
    {
        PaintRect(x0, centerY - roadHalfWidth - sidewalkWidth, x1 - x0 + 1, sidewalkWidth, TileType.Pavement);
        PaintRect(x0, centerY + roadHalfWidth + 1, x1 - x0 + 1, sidewalkWidth, TileType.Pavement);
        PaintRect(x0, centerY - roadHalfWidth, x1 - x0 + 1, roadHalfWidth * 2 + 1, TileType.Asphalt);
    }

    public void PaintRoadVertical(int centerX, int y0, int y1, int roadHalfWidth = 3, int sidewalkWidth = 2)
    {
        PaintRect(centerX - roadHalfWidth - sidewalkWidth, y0, sidewalkWidth, y1 - y0 + 1, TileType.Pavement);
        PaintRect(centerX + roadHalfWidth + 1, y0, sidewalkWidth, y1 - y0 + 1, TileType.Pavement);
        PaintRect(centerX - roadHalfWidth, y0, roadHalfWidth * 2 + 1, y1 - y0 + 1, TileType.Asphalt);
    }

    public void PaintPathHorizontal(int centerY, int x0, int x1)
    {
        PaintRect(x0, centerY - 1, x1 - x0 + 1, 3, TileType.Dirt);
    }

    public void PaintPathVertical(int centerX, int y0, int y1)
    {
        PaintRect(centerX - 1, y0, 3, y1 - y0 + 1, TileType.Dirt);
    }

    public void PaintBuilding(int x, int y, int width, int height, bool ruined = false, int doorWorldX = int.MinValue)
    {
        PaintRect(x, y, width, height, TileType.BuildingFloor);
        for (var ix = x; ix < x + width; ix++)
        {
            SetTile(ix, y, TileType.BuildingWall);
            SetTile(ix, y + height - 1, TileType.BuildingWall);
        }
        for (var iy = y; iy < y + height; iy++)
        {
            SetTile(x, iy, TileType.BuildingWall);
            SetTile(x + width - 1, iy, TileType.BuildingWall);
        }
        var doorX = doorWorldX == int.MinValue ? x + width / 2 : doorWorldX;
        SetTile(doorX, y + height - 1, TileType.BuildingFloor);
        if (doorX + 1 < x + width - 1) SetTile(doorX + 1, y + height - 1, TileType.BuildingFloor);

        if (!ruined) return;
        var rubbleRects = new[]
        {
            new Rectangle(x + 1, y + 1, 3, 2),
            new Rectangle(x + width - 5, y + 2, 4, 3),
            new Rectangle(x + width / 2 - 2, y + height / 2, 5, 3)
        };
        foreach (var r in rubbleRects) PaintRect(r.X, r.Y, r.Width, r.Height, TileType.Rubble);
        for (var i = 0; i < width; i += 5)
        {
            SetTile(x + i, y, TileType.Rubble);
            if (i % 2 == 0) SetTile(x + width - 1, y + i % height, TileType.Rubble);
        }
    }

    public void AddResource(ResourceKind kind, int tileX, int tileY, int durability = 0)
    {
        var pos = TileCenter(tileX, tileY);
        Resources.Add(new MapResourcePlacement($"{Id}:r:{kind}:{tileX}:{tileY}", kind, pos, durability > 0 ? durability : DefaultDurability(kind)));
    }

    public void AddDecoration(DecorationKind kind, int tileX, int tileY, float offsetX = 0f, float offsetY = 0f, float scale = 1f, bool blocksMovement = false)
    {
        Decorations.Add(new MapDecorationPlacement($"{Id}:d:{kind}:{tileX}:{tileY}:{Decorations.Count}", kind, TileCenter(tileX, tileY) + new Vector2(offsetX, offsetY), scale, blocksMovement));
    }

    public void AddZombieSpawn(int tileX, int tileY)
    {
        ZombieSpawnPoints.Add(TileCenter(tileX, tileY));
    }

    public static Vector2 TileCenter(int tileX, int tileY) => new((tileX + 0.5f) * Balance.TileSize, (tileY + 0.5f) * Balance.TileSize);

    private static int DefaultDurability(ResourceKind kind) => kind switch
    {
        ResourceKind.Tree => 5,
        ResourceKind.WreckedCar => 9,
        ResourceKind.Barrel => 3,
        ResourceKind.ScrapPile => 4,
        ResourceKind.AmmoCache => 3,
        ResourceKind.MedicalCache => 3,
        _ => 2
    };
}

public readonly record struct MapResourcePlacement(string Id, ResourceKind Kind, Vector2 Position, int Durability);
public readonly record struct MapDecorationPlacement(string Id, DecorationKind Kind, Vector2 Position, float Scale, bool BlocksMovement);
public readonly record struct WorldMapSummary(string Id, string Name, string Description);

public static class WorldMapCatalog
{
    public const string DefaultMapId = "crossroads";

    public static IReadOnlyList<WorldMapSummary> Summaries { get; } = new[]
    {
        new WorldMapSummary("crossroads", "OVERGROWN CROSSROADS", "Balanced survivor start: park, blocked road, small shops, ambulance cache."),
        new WorldMapSummary("industrial", "INDUSTRIAL YARD", "Harder start: fenced worksite, containers, scrap, barrels, warehouse ruins."),
        new WorldMapSummary("downtown", "DOWNTOWN BREAKOUT", "Dense urban map: streets, offices, parking wrecks, high loot and higher danger.")
    };

    public static WorldMapSummary Summary(string id)
    {
        foreach (var summary in Summaries)
        {
            if (string.Equals(summary.Id, id, StringComparison.OrdinalIgnoreCase)) return summary;
        }
        return Summaries[0];
    }

    public static WorldMapDefinition Create(string id)
    {
        return (string.IsNullOrWhiteSpace(id) ? DefaultMapId : id).ToLowerInvariant() switch
        {
            "industrial" => BuildIndustrial(),
            "downtown" => BuildDowntown(),
            _ => BuildCrossroads()
        };
    }

    public static Vector2 SpawnFor(string id) => Create(id).Spawn;

    private static WorldMapDefinition BuildCrossroads()
    {
        var m = NewMap("crossroads", "OVERGROWN CROSSROADS", "A hand-authored neighborhood crossroads with parkland, survivor shelter, roadblock, and looted shops.", 148, 112, new Vector2(-19.5f * Balance.TileSize, 7.5f * Balance.TileSize));

        m.PaintRect(-70, -52, 58, 45, TileType.DryGrass);
        m.PaintRect(-18, -52, 40, 30, TileType.Pavement);
        m.PaintRect(22, -52, 44, 34, TileType.DryGrass);
        m.PaintRect(-70, -7, 48, 34, TileType.DryGrass);
        m.PaintRect(28, -7, 40, 34, TileType.DryGrass);
        m.PaintRoadHorizontal(0, -74, 73, 4, 2);
        m.PaintRoadVertical(18, -56, 55, 4, 2);
        m.PaintPathHorizontal(-22, -61, -28);
        m.PaintPathVertical(-44, -42, 25);
        m.PaintPathHorizontal(26, -62, 8);
        m.PaintPathVertical(44, -28, 31);

        m.PaintBuilding(-16, -49, 21, 16, ruined: false, doorWorldX: -6);
        m.PaintBuilding(38, -47, 24, 18, ruined: true, doorWorldX: 48);
        m.PaintBuilding(-68, 23, 26, 18, ruined: true, doorWorldX: -55);
        m.PaintBuilding(34, 25, 28, 18, ruined: false, doorWorldX: 47);

        m.PaintRect(-10, -4, 7, 9, TileType.Rubble);
        m.PaintRect(-1, -5, 8, 8, TileType.Rubble);
        m.PaintRect(7, -3, 7, 5, TileType.Rubble);
        m.PaintRect(25, 4, 9, 5, TileType.Rubble);
        m.PaintRect(26, 9, 5, 4, TileType.Rubble);
        m.PaintRect(30, -18, 5, 9, TileType.Rubble);
        m.PaintRect(-37, -3, 12, 5, TileType.Rubble);

        AddParkCluster(m, -53, -32);
        AddParkCluster(m, -58, 14);
        AddParkCluster(m, 43, -26);
        AddParkCluster(m, 52, 18);
        AddRoadScene(m);
        AddShopInteriorLoot(m, -7, -41);
        AddShopInteriorLoot(m, 47, -39);
        AddShopInteriorLoot(m, 46, 34);

        m.AddResource(ResourceKind.WreckedCar, 3, -10);
        m.AddResource(ResourceKind.WreckedCar, 8, -7);
        m.AddResource(ResourceKind.WreckedCar, 13, -5);
        m.AddResource(ResourceKind.WreckedCar, 23, 4);
        m.AddResource(ResourceKind.ScrapPile, -5, -2);
        m.AddResource(ResourceKind.ScrapPile, 5, 2);
        m.AddResource(ResourceKind.Barrel, 42, 14);
        m.AddResource(ResourceKind.FoodCache, -11, -41);
        m.AddResource(ResourceKind.WaterCache, 50, 34);
        m.AddResource(ResourceKind.MedicalCache, 17, -16);
        m.AddResource(ResourceKind.AmmoCache, 52, -38);

        m.AddZombieSpawn(57, -14);
        m.AddZombieSpawn(45, 40);
        m.AddZombieSpawn(-62, 36);
        m.AddZombieSpawn(-3, -40);
        return m;
    }

    private static WorldMapDefinition BuildIndustrial()
    {
        var m = NewMap("industrial", "INDUSTRIAL YARD", "A curated industrial block with fences, containers, salvage piles, and a warehouse holdout.", 154, 116, new Vector2(-36.5f * Balance.TileSize, 17.5f * Balance.TileSize));
        m.PaintRect(-75, -55, 150, 110, TileType.Dirt);
        m.PaintRoadHorizontal(26, -77, 76, 4, 2);
        m.PaintRoadVertical(-45, -57, 55, 4, 2);
        m.PaintRect(-34, -45, 65, 48, TileType.Pavement);
        m.PaintRect(34, -42, 31, 38, TileType.Rubble);
        m.PaintRect(-71, -42, 19, 62, TileType.DryGrass);
        m.PaintBuilding(-31, -41, 34, 20, ruined: false, doorWorldX: -14);
        m.PaintBuilding(7, -38, 22, 16, ruined: true, doorWorldX: 17);
        m.PaintBuilding(-18, 2, 46, 20, ruined: true, doorWorldX: 4);

        for (var x = -37; x <= 36; x += 6)
        {
            m.AddDecoration(DecorationKind.Fence, x, -47, scale: 1.1f, blocksMovement: true);
            m.AddDecoration(DecorationKind.Fence, x, 5, scale: 1.1f, blocksMovement: true);
        }
        for (var y = -42; y <= 3; y += 6)
        {
            m.AddDecoration(DecorationKind.Fence, -39, y, scale: 1.1f, blocksMovement: true);
            m.AddDecoration(DecorationKind.Fence, 38, y, scale: 1.1f, blocksMovement: true);
        }

        for (var i = 0; i < 9; i++)
        {
            var x = -24 + i * 7;
            m.AddDecoration(i % 3 == 0 ? DecorationKind.Container : DecorationKind.BrickDebris, x, -10 + i % 2 * 5, scale: 1f);
            m.AddResource(ResourceKind.ScrapPile, x + 1, -7 + i % 3 * 4);
        }
        for (var i = 0; i < 8; i++) m.AddResource(i % 2 == 0 ? ResourceKind.Barrel : ResourceKind.ScrapPile, 38 + i * 4, -30 + i % 4 * 7);
        m.AddResource(ResourceKind.AmmoCache, -19, -33);
        m.AddResource(ResourceKind.MedicalCache, -3, -31);
        m.AddResource(ResourceKind.WaterCache, -9, 13);
        m.AddResource(ResourceKind.FoodCache, 5, 12);
        m.AddResource(ResourceKind.WreckedCar, -48, 26);
        m.AddResource(ResourceKind.WreckedCar, -42, 31);

        m.AddDecoration(DecorationKind.GarbageBin, -39, 15);
        m.AddDecoration(DecorationKind.Manhole, -45, 3, scale: 1.2f);
        m.AddDecoration(DecorationKind.Hydrant, -50, 16);
        m.AddDecoration(DecorationKind.AirVent, -25, -36);
        m.AddDecoration(DecorationKind.AirVent, -10, -36);
        m.AddDecoration(DecorationKind.RoofHole, 14, -31);
        m.AddDecoration(DecorationKind.DestroyedWall, 10, 5);
        m.AddDecoration(DecorationKind.DestroyedWall, 24, 15);
        for (var i = 0; i < 18; i++) m.AddDecoration(DecorationKind.GrassTuft, -67 + i % 5 * 4, -34 + i * 4 % 56, scale: 0.8f);

        m.AddZombieSpawn(49, -37);
        m.AddZombieSpawn(53, 9);
        m.AddZombieSpawn(-63, -11);
        m.AddZombieSpawn(15, 42);
        return m;
    }

    private static WorldMapDefinition BuildDowntown()
    {
        var m = NewMap("downtown", "DOWNTOWN BREAKOUT", "A handcrafted city block with shops, alleys, parking wrecks, and high pressure zombie approaches.", 160, 120, new Vector2(-5.5f * Balance.TileSize, 19.5f * Balance.TileSize));
        m.PaintRect(-78, -58, 156, 116, TileType.Pavement);
        m.PaintRoadHorizontal(18, -80, 79, 4, 2);
        m.PaintRoadHorizontal(-31, -80, 79, 4, 2);
        m.PaintRoadVertical(-39, -60, 59, 4, 2);
        m.PaintRoadVertical(34, -60, 59, 4, 2);
        m.PaintRect(-24, -16, 44, 28, TileType.DryGrass);
        m.PaintPathHorizontal(-3, -20, 19);
        m.PaintPathVertical(-2, -16, 11);

        m.PaintBuilding(-73, -55, 28, 20, ruined: true, doorWorldX: -60);
        m.PaintBuilding(-28, -55, 35, 19, ruined: false, doorWorldX: -11);
        m.PaintBuilding(41, -55, 31, 20, ruined: true, doorWorldX: 55);
        m.PaintBuilding(-73, -18, 27, 23, ruined: false, doorWorldX: -59);
        m.PaintBuilding(46, -15, 27, 22, ruined: false, doorWorldX: 60);
        m.PaintBuilding(-70, 31, 34, 23, ruined: true, doorWorldX: -54);
        m.PaintBuilding(41, 31, 31, 23, ruined: true, doorWorldX: 55);

        for (var x = -19; x <= 16; x += 7) m.AddDecoration(DecorationKind.Bench, x, 9, scale: 1.1f);
        AddParkCluster(m, -15, -6);
        AddParkCluster(m, 11, -5);
        for (var i = 0; i < 12; i++) m.AddDecoration(DecorationKind.Manhole, -39 + i % 2 * 73, -48 + i * 9, scale: 1.2f);
        for (var i = 0; i < 16; i++) m.AddDecoration(DecorationKind.GarbageBin, -62 + i % 4 * 41, -23 + i / 4 * 21, scale: 0.9f);

        var cars = new[] { new Point(-42, -28), new Point(-35, -28), new Point(27, -35), new Point(35, -35), new Point(33, 20), new Point(41, 22), new Point(-45, 21), new Point(-38, 24) };
        foreach (var c in cars) m.AddResource(ResourceKind.WreckedCar, c.X, c.Y);
        for (var i = 0; i < 15; i++) m.AddResource(ResourceKind.ScrapPile, -66 + i % 5 * 31, -45 + i / 5 * 37);
        m.AddResource(ResourceKind.FoodCache, -13, -46);
        m.AddResource(ResourceKind.FoodCache, 55, -45);
        m.AddResource(ResourceKind.WaterCache, -58, -9);
        m.AddResource(ResourceKind.MedicalCache, 59, -6);
        m.AddResource(ResourceKind.AmmoCache, 55, 42);
        m.AddResource(ResourceKind.MedicalCache, -54, 41);

        m.AddZombieSpawn(-69, -47);
        m.AddZombieSpawn(64, -43);
        m.AddZombieSpawn(-63, 43);
        m.AddZombieSpawn(65, 43);
        m.AddZombieSpawn(8, -44);
        return m;
    }

    private static WorldMapDefinition NewMap(string id, string name, string description, int width, int height, Vector2 spawn)
    {
        var m = new WorldMapDefinition(id, name, description, width, height, spawn);
        m.PaintRect(m.MinTileX, m.MinTileY, m.Width, m.Height, TileType.DryGrass);
        m.PaintRect(m.MinTileX, m.MinTileY, m.Width, 3, TileType.Rubble);
        m.PaintRect(m.MinTileX, m.MaxTileY - 2, m.Width, 3, TileType.Rubble);
        m.PaintRect(m.MinTileX, m.MinTileY, 3, m.Height, TileType.Rubble);
        m.PaintRect(m.MaxTileX - 2, m.MinTileY, 3, m.Height, TileType.Rubble);
        return m;
    }

    private static void AddParkCluster(WorldMapDefinition m, int x, int y)
    {
        m.AddResource(ResourceKind.Tree, x, y);
        m.AddResource(ResourceKind.Tree, x + 10, y + 12);
        m.AddDecoration(DecorationKind.Bush, x + 7, y - 5, scale: 1.1f);
        m.AddDecoration(DecorationKind.Bush, x - 6, y + 9, scale: 1f);
        m.AddDecoration(DecorationKind.GrassTuft, x + 3, y + 6, scale: 0.9f);
        m.AddDecoration(DecorationKind.GrassTuft, x + 15, y + 1, scale: 0.9f);
        m.AddDecoration(DecorationKind.TireStack, x - 9, y + 17, scale: 1.1f);
    }

    private static void AddRoadScene(WorldMapDefinition m)
    {
        m.AddDecoration(DecorationKind.Hydrant, -24, -8, scale: 1f);
        m.AddDecoration(DecorationKind.Hydrant, 24, 12, scale: 1f);
        m.AddDecoration(DecorationKind.Manhole, 16, 9, scale: 1.3f);
        m.AddDecoration(DecorationKind.GarbageBin, -20, 8, scale: 0.9f);
        m.AddDecoration(DecorationKind.Bench, -28, -15, scale: 1.1f);
        m.AddDecoration(DecorationKind.Bench, 39, -16, scale: 1.1f);
        m.AddDecoration(DecorationKind.Container, 57, 3, scale: 0.85f);
    }

    private static void AddShopInteriorLoot(WorldMapDefinition m, int x, int y)
    {
        m.AddDecoration(DecorationKind.Door, x, y + 8, scale: 1f);
        m.AddDecoration(DecorationKind.AirVent, x + 5, y - 3, scale: 1f);
        m.AddDecoration(DecorationKind.Cardboard, x - 3, y + 2, scale: 1f);
        m.AddDecoration(DecorationKind.GarbageBin, x + 8, y + 1, scale: 0.9f);
    }
}
