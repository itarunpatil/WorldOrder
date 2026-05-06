using Microsoft.Xna.Framework;
using WorldOrder.Core;

namespace WorldOrder.World;

public enum TileType
{
    Dirt,
    DryGrass,
    Rubble,
    Asphalt,
    Pavement,
    BuildingFloor,
    BuildingWall,
    Water
}

public enum ResourceKind
{
    Tree,
    ScrapPile,
    FoodCache,
    WaterCache,
    MedicalCache,
    AmmoCache,
    WreckedCar,
    Barrel
}

public enum DecorationKind
{
    GrassTuft,
    Bush,
    TireStack,
    Cardboard,
    GarbageBin,
    Hydrant,
    Manhole,
    Bench,
    Container,
    AirVent,
    Door,
    DestroyedWall,
    BrickDebris,
    RoofHole,
    Fence
}

public readonly record struct ChunkCoord(int X, int Y)
{
    public static ChunkCoord FromWorld(Vector2 world)
    {
        var tileX = MathTools.FloorDiv((int)MathF.Floor(world.X), Balance.TileSize);
        var tileY = MathTools.FloorDiv((int)MathF.Floor(world.Y), Balance.TileSize);
        return new ChunkCoord(MathTools.FloorDiv(tileX, Balance.ChunkSize), MathTools.FloorDiv(tileY, Balance.ChunkSize));
    }
}

public sealed class ResourceNode
{
    public ResourceNode(string id, ResourceKind kind, Vector2 position, int durability)
    {
        Id = id;
        Kind = kind;
        Position = position;
        Durability = durability;
        MaxDurability = durability;
    }

    public string Id { get; }
    public ResourceKind Kind { get; }
    public Vector2 Position { get; }
    public int Durability { get; private set; }
    public int MaxDurability { get; }
    public bool IsDestroyed => Durability <= 0;
    public RectangleF Bounds => new(Position.X - 14, Position.Y - 18, 28, 34);

    public bool Damage(int amount)
    {
        Durability = Math.Max(0, Durability - amount);
        return IsDestroyed;
    }
}

public sealed class DecorationNode
{
    public DecorationNode(string id, DecorationKind kind, Vector2 position, float scale, bool blocksMovement = false)
    {
        Id = id;
        Kind = kind;
        Position = position;
        Scale = scale;
        BlocksMovement = blocksMovement;
    }

    public string Id { get; }
    public DecorationKind Kind { get; }
    public Vector2 Position { get; }
    public float Scale { get; }
    public bool BlocksMovement { get; }
    public RectangleF Bounds => new(Position.X - 13 * Scale, Position.Y - 13 * Scale, 26 * Scale, 26 * Scale);
}

public sealed class Chunk
{
    public Chunk(ChunkCoord coord)
    {
        Coord = coord;
        Tiles = new TileType[Balance.ChunkSize * Balance.ChunkSize];
    }

    public ChunkCoord Coord { get; }
    public TileType[] Tiles { get; }
    public List<ResourceNode> Resources { get; } = new();
    public List<DecorationNode> Decorations { get; } = new();
    public Rectangle WorldRectangle => new(Coord.X * Balance.ChunkSize * Balance.TileSize, Coord.Y * Balance.ChunkSize * Balance.TileSize, Balance.ChunkSize * Balance.TileSize, Balance.ChunkSize * Balance.TileSize);

    public TileType GetLocal(int x, int y) => Tiles[y * Balance.ChunkSize + x];
    public void SetLocal(int x, int y, TileType type) => Tiles[y * Balance.ChunkSize + x] = type;
}
