using Microsoft.Xna.Framework;
using WorldOrder.Core;

namespace WorldOrder.World;

public sealed class WorldGenerator
{
    private const int CityCell = 52;
    private const int RoadHalfWidth = 3;
    private const int SidewalkWidth = 3;
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
        PopulateDecorations(chunk);
        return chunk;
    }

    public TileType TileAt(int tileX, int tileY)
    {
        if (IsSpawnPlaza(tileX, tileY))
        {
            if (Math.Abs(tileX) <= 3 || Math.Abs(tileY) <= 3) return TileType.Asphalt;
            return TileType.Pavement;
        }

        var road = RoadAt(tileX, tileY);
        if (road.OnRoad) return TileType.Asphalt;
        if (road.OnSidewalk) return TileType.Pavement;

        var building = BuildingAt(tileX, tileY);
        if (building.IsWall) return TileType.BuildingWall;
        if (building.IsFloor) return TileType.BuildingFloor;

        var waterNoise = FractalNoise(tileX, tileY, 28, _seed + 1200);
        if (waterNoise < 0.105f && !road.NearRoad && Math.Abs(tileX) + Math.Abs(tileY) > 34) return TileType.Water;

        var biome = FractalNoise(tileX, tileY, 46, _seed + 2400);
        var debris = ValueNoise(tileX, tileY, 11, _seed + 3400);
        if (debris > 0.865f && biome < 0.64f) return TileType.Rubble;
        return biome > 0.55f ? TileType.DryGrass : TileType.Dirt;
    }

    public bool BlocksMovement(int tileX, int tileY)
    {
        var tile = TileAt(tileX, tileY);
        return tile == TileType.BuildingWall || tile == TileType.Water;
    }

    private void PopulateResources(Chunk chunk, ISet<string> depletedNodes)
    {
        for (var i = 0; i < 22; i++)
        {
            var localSeed = _seed + i * 104729;
            var lx = Hashing.Range(chunk.Coord.X, chunk.Coord.Y, localSeed, 1, Balance.ChunkSize - 1);
            var ly = Hashing.Range(chunk.Coord.X, chunk.Coord.Y, localSeed + 53, 1, Balance.ChunkSize - 1);
            var wx = chunk.Coord.X * Balance.ChunkSize + lx;
            var wy = chunk.Coord.Y * Balance.ChunkSize + ly;
            if (IsSpawnPlaza(wx, wy)) continue;

            var tile = TileAt(wx, wy);
            if (tile == TileType.Water || tile == TileType.BuildingWall) continue;

            var roll = Hashing.Unit(wx, wy, _seed + 7000);
            ResourceKind? kind = null;
            if (tile == TileType.DryGrass && roll > 0.60f) kind = ResourceKind.Tree;
            else if (tile == TileType.Dirt && roll > 0.88f) kind = ResourceKind.Barrel;
            else if (tile == TileType.Rubble && roll > 0.52f) kind = ResourceKind.ScrapPile;
            else if (tile == TileType.Pavement && roll > 0.76f) kind = roll > 0.90f ? ResourceKind.WreckedCar : ResourceKind.ScrapPile;
            else if (tile == TileType.Asphalt && roll > 0.82f) kind = ResourceKind.WreckedCar;
            else if (tile == TileType.BuildingFloor && roll > 0.56f) kind = BuildingLoot(wx, wy, roll);
            if (kind is null) continue;

            var id = $"{wx}:{wy}:{kind.Value}";
            if (depletedNodes.Contains(id)) continue;
            var pos = new Vector2((wx + 0.5f) * Balance.TileSize, (wy + 0.5f) * Balance.TileSize);
            chunk.Resources.Add(new ResourceNode(id, kind.Value, pos, Durability(kind.Value)));
        }
    }

    private void PopulateDecorations(Chunk chunk)
    {
        for (var i = 0; i < 42; i++)
        {
            var salt = _seed + 50000 + i * 7919;
            var lx = Hashing.Range(chunk.Coord.X, chunk.Coord.Y, salt, 0, Balance.ChunkSize);
            var ly = Hashing.Range(chunk.Coord.X, chunk.Coord.Y, salt + 17, 0, Balance.ChunkSize);
            var wx = chunk.Coord.X * Balance.ChunkSize + lx;
            var wy = chunk.Coord.Y * Balance.ChunkSize + ly;
            if (IsSpawnPlaza(wx, wy) && Hashing.Unit(wx, wy, salt + 20) < 0.75f) continue;
            if (chunk.Resources.Any(r => Vector2.DistanceSquared(r.Position, new Vector2((wx + 0.5f) * Balance.TileSize, (wy + 0.5f) * Balance.TileSize)) < 26f * 26f)) continue;

            var tile = TileAt(wx, wy);
            if (tile == TileType.Water || tile == TileType.BuildingWall) continue;
            var roll = Hashing.Unit(wx, wy, salt + 99);
            DecorationKind? kind = tile switch
            {
                TileType.DryGrass => roll < 0.48f ? DecorationKind.GrassTuft : roll < 0.58f ? DecorationKind.Bush : roll < 0.64f ? DecorationKind.TireStack : null,
                TileType.Dirt => roll < 0.28f ? DecorationKind.GrassTuft : roll < 0.38f ? DecorationKind.Cardboard : roll < 0.46f ? DecorationKind.BrickDebris : null,
                TileType.Rubble => roll < 0.40f ? DecorationKind.BrickDebris : roll < 0.52f ? DecorationKind.DestroyedWall : roll < 0.60f ? DecorationKind.Cardboard : null,
                TileType.Asphalt => roll < 0.16f ? DecorationKind.Manhole : roll < 0.23f ? DecorationKind.Hydrant : roll < 0.30f ? DecorationKind.GarbageBin : null,
                TileType.Pavement => roll < 0.16f ? DecorationKind.Bench : roll < 0.26f ? DecorationKind.GarbageBin : roll < 0.32f ? DecorationKind.Hydrant : null,
                TileType.BuildingFloor => roll < 0.17f ? DecorationKind.AirVent : roll < 0.25f ? DecorationKind.RoofHole : roll < 0.33f ? DecorationKind.Door : null,
                _ => null
            };
            if (kind is null) continue;

            var jitterX = (Hashing.Unit(wx, wy, salt + 7) - 0.5f) * Balance.TileSize * 0.42f;
            var jitterY = (Hashing.Unit(wx, wy, salt + 8) - 0.5f) * Balance.TileSize * 0.42f;
            var scale = 1.05f + Hashing.Unit(wx, wy, salt + 9) * 0.42f;
            var id = $"d:{wx}:{wy}:{kind.Value}:{i}";
            var pos = new Vector2((wx + 0.5f) * Balance.TileSize + jitterX, (wy + 0.5f) * Balance.TileSize + jitterY);
            chunk.Decorations.Add(new DecorationNode(id, kind.Value, pos, scale));
        }
    }

    private ResourceKind BuildingLoot(int tileX, int tileY, float roll)
    {
        var room = Hashing.Unit(MathTools.FloorDiv(tileX, 4), MathTools.FloorDiv(tileY, 4), _seed + 8100);
        if (roll > 0.91f) return ResourceKind.AmmoCache;
        if (room > 0.78f) return ResourceKind.MedicalCache;
        if (room > 0.48f) return ResourceKind.FoodCache;
        return ResourceKind.WaterCache;
    }

    private int Durability(ResourceKind kind) => kind switch
    {
        ResourceKind.Tree => 5,
        ResourceKind.WreckedCar => 8,
        ResourceKind.Barrel => 3,
        ResourceKind.ScrapPile => 4,
        ResourceKind.AmmoCache => 3,
        _ => 2
    };

    private bool IsSpawnPlaza(int tileX, int tileY) => Math.Abs(tileX) <= 14 && Math.Abs(tileY) <= 14;

    private RoadInfo RoadAt(int tileX, int tileY)
    {
        var mx = MathTools.PositiveModulo(tileX, CityCell);
        var my = MathTools.PositiveModulo(tileY, CityCell);
        var dx = Math.Min(mx, CityCell - mx);
        var dy = Math.Min(my, CityCell - my);
        var onVertical = dx <= RoadHalfWidth;
        var onHorizontal = dy <= RoadHalfWidth;
        var nearVertical = dx <= RoadHalfWidth + SidewalkWidth;
        var nearHorizontal = dy <= RoadHalfWidth + SidewalkWidth;
        return new RoadInfo(onVertical || onHorizontal, !onVertical && !onHorizontal && (nearVertical || nearHorizontal), nearVertical || nearHorizontal);
    }

    private BuildingInfo BuildingAt(int tileX, int tileY)
    {
        if (Math.Abs(tileX) <= 18 && Math.Abs(tileY) <= 18) return default;
        var road = RoadAt(tileX, tileY);
        if (road.NearRoad) return default;

        var cellX = MathTools.FloorDiv(tileX, CityCell);
        var cellY = MathTools.FloorDiv(tileY, CityCell);
        var localX = MathTools.PositiveModulo(tileX, CityCell);
        var localY = MathTools.PositiveModulo(tileY, CityCell);
        if (localX < 9 || localX > CityCell - 10 || localY < 9 || localY > CityCell - 10) return default;

        var parcelX = localX < CityCell / 2 ? 0 : 1;
        var parcelY = localY < CityCell / 2 ? 0 : 1;
        var parcelSeedX = cellX * 2 + parcelX;
        var parcelSeedY = cellY * 2 + parcelY;
        if (Hashing.Unit(parcelSeedX, parcelSeedY, _seed + 4100) < 0.24f) return default;

        var baseX = parcelX == 0 ? 8 : 28;
        var baseY = parcelY == 0 ? 8 : 28;
        var width = 11 + Hashing.Range(parcelSeedX, parcelSeedY, _seed + 4200, 0, 7);
        var height = 10 + Hashing.Range(parcelSeedX, parcelSeedY, _seed + 4300, 0, 7);
        var offsetX = Hashing.Range(parcelSeedX, parcelSeedY, _seed + 4400, 0, 4);
        var offsetY = Hashing.Range(parcelSeedX, parcelSeedY, _seed + 4500, 0, 4);
        var x0 = baseX + offsetX;
        var y0 = baseY + offsetY;
        var x1 = Math.Min(x0 + width, parcelX == 0 ? 24 : 45);
        var y1 = Math.Min(y0 + height, parcelY == 0 ? 24 : 45);
        if (localX < x0 || localX > x1 || localY < y0 || localY > y1) return default;

        var border = localX == x0 || localX == x1 || localY == y0 || localY == y1;
        if (!border) return new BuildingInfo(false, true);

        var doorX = (x0 + x1) / 2;
        var hasDoor = localY == y1 && Math.Abs(localX - doorX) <= 1;
        return hasDoor ? new BuildingInfo(false, true) : new BuildingInfo(true, false);
    }

    private float FractalNoise(int x, int y, int scale, int seed)
    {
        var a = ValueNoise(x, y, scale, seed);
        var b = ValueNoise(x, y, Math.Max(6, scale / 2), seed + 101) * 0.5f;
        var c = ValueNoise(x, y, Math.Max(4, scale / 4), seed + 202) * 0.25f;
        return (a + b + c) / 1.75f;
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

    private readonly record struct RoadInfo(bool OnRoad, bool OnSidewalk, bool NearRoad);
    private readonly record struct BuildingInfo(bool IsWall, bool IsFloor);
}
