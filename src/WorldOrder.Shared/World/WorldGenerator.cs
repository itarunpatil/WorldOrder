using Microsoft.Xna.Framework;
using WorldOrder.Core;

namespace WorldOrder.World;

public sealed class WorldGenerator
{
    private readonly int _seed;

    public WorldGenerator(int seed) => _seed = seed;

    public Chunk GenerateChunk(ChunkCoord coord, ISet<string> depletedNodes)
    {
        var chunk = new Chunk(coord);
        for (var ly = 0; ly < Balance.ChunkSize; ly++)
        {
            for (var lx = 0; lx < Balance.ChunkSize; lx++)
            {
                var wx = coord.X * Balance.ChunkSize + lx;
                var wy = coord.Y * Balance.ChunkSize + ly;
                chunk.SetLocal(lx, ly, TileAt(wx, wy));
            }
        }
        PopulateResources(chunk, depletedNodes);
        return chunk;
    }

    public TileType TileAt(int tileX, int tileY)
    {
        if (IsRoad(tileX, tileY)) return TileType.Asphalt;
        if (IsBuildingWall(tileX, tileY)) return TileType.BuildingWall;
        if (IsBuildingFloor(tileX, tileY)) return TileType.BuildingFloor;
        var n = ValueNoise(tileX, tileY, 34, _seed + 1000);
        if (n < 0.12f && !NearRoad(tileX, tileY, 3)) return TileType.Water;
        if (n < 0.32f) return TileType.Rubble;
        return n < 0.62f ? TileType.Dirt : TileType.DryGrass;
    }

    public bool BlocksMovement(int tileX, int tileY)
    {
        var tile = TileAt(tileX, tileY);
        return tile == TileType.BuildingWall || tile == TileType.Water;
    }

    private void PopulateResources(Chunk chunk, ISet<string> depletedNodes)
    {
        var attempts = 18;
        for (var i = 0; i < attempts; i++)
        {
            var localSeed = _seed + i * 104729;
            var lx = Hashing.Range(chunk.Coord.X, chunk.Coord.Y, localSeed, 2, Balance.ChunkSize - 2);
            var ly = Hashing.Range(chunk.Coord.X, chunk.Coord.Y, localSeed + 53, 2, Balance.ChunkSize - 2);
            var wx = chunk.Coord.X * Balance.ChunkSize + lx;
            var wy = chunk.Coord.Y * Balance.ChunkSize + ly;
            var tile = TileAt(wx, wy);
            if (tile == TileType.Water || tile == TileType.BuildingWall) continue;

            var roll = Hashing.Unit(wx, wy, _seed + 7000);
            ResourceKind? kind = null;
            if (tile == TileType.DryGrass && roll > 0.63f) kind = ResourceKind.Tree;
            else if ((tile == TileType.Rubble || tile == TileType.Pavement) && roll > 0.68f) kind = ResourceKind.ScrapPile;
            else if (tile == TileType.Asphalt && roll > 0.78f) kind = ResourceKind.WreckedCar;
            else if (tile == TileType.BuildingFloor && roll > 0.64f) kind = roll > 0.86f ? ResourceKind.MedicalCache : ResourceKind.FoodCache;
            else if (tile == TileType.Dirt && roll > 0.87f) kind = ResourceKind.Barrel;
            if (kind is null) continue;

            var id = $"{wx}:{wy}:{kind.Value}";
            if (depletedNodes.Contains(id)) continue;
            var pos = new Vector2((wx + 0.5f) * Balance.TileSize, (wy + 0.5f) * Balance.TileSize);
            chunk.Resources.Add(new ResourceNode(id, kind.Value, pos, Durability(kind.Value)));
        }
    }

    private int Durability(ResourceKind kind) => kind switch
    {
        ResourceKind.Tree => 5,
        ResourceKind.WreckedCar => 8,
        ResourceKind.Barrel => 3,
        ResourceKind.ScrapPile => 4,
        _ => 2
    };

    private bool IsRoad(int tileX, int tileY)
    {
        var mx = MathTools.PositiveModulo(tileX, 54);
        var my = MathTools.PositiveModulo(tileY, 54);
        return mx is >= 24 and <= 30 || my is >= 24 and <= 30;
    }

    private bool NearRoad(int tileX, int tileY, int radius)
    {
        var mx = MathTools.PositiveModulo(tileX, 54);
        var my = MathTools.PositiveModulo(tileY, 54);
        return mx >= 24 - radius && mx <= 30 + radius || my >= 24 - radius && my <= 30 + radius;
    }

    private bool IsBuildingFloor(int tileX, int tileY)
    {
        if (!NearRoad(tileX, tileY, 8)) return false;
        var blockX = MathTools.FloorDiv(tileX, 18);
        var blockY = MathTools.FloorDiv(tileY, 18);
        if (Hashing.Unit(blockX, blockY, _seed + 300) < 0.54f) return false;
        var lx = MathTools.PositiveModulo(tileX, 18);
        var ly = MathTools.PositiveModulo(tileY, 18);
        return lx is >= 3 and <= 13 && ly is >= 3 and <= 13;
    }

    private bool IsBuildingWall(int tileX, int tileY)
    {
        if (!IsBuildingFloor(tileX, tileY)) return false;
        var lx = MathTools.PositiveModulo(tileX, 18);
        var ly = MathTools.PositiveModulo(tileY, 18);
        if (ly == 13 && lx is >= 8 and <= 10) return false;
        return lx == 3 || lx == 13 || ly == 3 || ly == 13;
    }

    private float ValueNoise(int x, int y, int scale, int seed)
    {
        var x0 = MathTools.FloorDiv(x, scale);
        var y0 = MathTools.FloorDiv(y, scale);
        var tx = MathTools.PositiveModulo(x, scale) / (float)scale;
        var ty = MathTools.PositiveModulo(y, scale) / (float)scale;
        tx = tx * tx * (3f - 2f * tx);
        ty = ty * ty * (3f - 2f * ty);
        var a = Hashing.Unit(x0, y0, seed);
        var b = Hashing.Unit(x0 + 1, y0, seed);
        var c = Hashing.Unit(x0, y0 + 1, seed);
        var d = Hashing.Unit(x0 + 1, y0 + 1, seed);
        var ab = MathHelper.Lerp(a, b, tx);
        var cd = MathHelper.Lerp(c, d, tx);
        return MathHelper.Lerp(ab, cd, ty);
    }
}
