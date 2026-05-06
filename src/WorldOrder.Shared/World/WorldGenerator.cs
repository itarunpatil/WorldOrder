using Microsoft.Xna.Framework;
using WorldOrder.Core;

namespace WorldOrder.World;

public enum RoadMarking
{
    None,
    HorizontalLane,
    VerticalLane,
    CrosswalkHorizontal,
    CrosswalkVertical
}

public sealed class WorldGenerator
{
    private const int CityCell = 64;
    private const int RoadHalfWidth = 3;
    private const int SidewalkWidth = 2;
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
        if (IsSpawnPlaza(tileX, tileY)) return SpawnTileAt(tileX, tileY);

        var road = RoadAt(tileX, tileY);
        if (road.OnRoad) return TileType.Asphalt;
        if (road.OnSidewalk) return TileType.Pavement;

        var building = BuildingAt(tileX, tileY);
        if (building.IsWall) return TileType.BuildingWall;
        if (building.IsFloor) return building.Ruined && Hashing.Unit(tileX, tileY, _seed + 7150) > 0.88f ? TileType.Rubble : TileType.BuildingFloor;

        var district = DistrictAt(tileX, tileY);
        var terrain = FractalNoise(tileX, tileY, 58, _seed + 2400);
        var detail = ValueNoise(tileX, tileY, 13, _seed + 3400);
        var water = FractalNoise(tileX, tileY, 44, _seed + 1200);

        if (district == District.Wild && water < 0.075f && Math.Abs(tileX) + Math.Abs(tileY) > 60) return TileType.Water;
        if (district == District.Industrial)
        {
            if (detail > 0.74f) return TileType.Rubble;
            if (terrain > 0.52f) return TileType.Pavement;
            return TileType.Dirt;
        }
        if (district == District.Ruins)
        {
            if (detail > 0.56f) return TileType.Rubble;
            if (terrain > 0.63f) return TileType.Pavement;
            return terrain > 0.38f ? TileType.Dirt : TileType.DryGrass;
        }
        if (district == District.City)
        {
            if (detail > 0.88f) return TileType.Rubble;
            if (terrain > 0.70f) return TileType.Pavement;
            return terrain > 0.36f ? TileType.Dirt : TileType.DryGrass;
        }

        if (detail > 0.92f) return TileType.Rubble;
        return terrain > 0.42f ? TileType.DryGrass : TileType.Dirt;
    }

    public bool BlocksMovement(int tileX, int tileY)
    {
        var tile = TileAt(tileX, tileY);
        return tile == TileType.BuildingWall || tile == TileType.Water;
    }

    public RoadMarking RoadMarkingAt(int tileX, int tileY)
    {
        var road = RoadAt(tileX, tileY);
        if (!road.OnRoad) return RoadMarking.None;
        var mx = MathTools.PositiveModulo(tileX, CityCell);
        var my = MathTools.PositiveModulo(tileY, CityCell);
        var dx = Math.Min(mx, CityCell - mx);
        var dy = Math.Min(my, CityCell - my);
        var onVertical = dx <= RoadHalfWidth;
        var onHorizontal = dy <= RoadHalfWidth;
        if (onVertical && onHorizontal)
        {
            if (Math.Abs(MathTools.PositiveModulo(tileY, 8) - 4) <= 1) return RoadMarking.CrosswalkHorizontal;
            if (Math.Abs(MathTools.PositiveModulo(tileX, 8) - 4) <= 1) return RoadMarking.CrosswalkVertical;
            return RoadMarking.None;
        }
        if (onVertical && dx == 0 && MathTools.PositiveModulo(tileY, 8) < 5) return RoadMarking.VerticalLane;
        if (onHorizontal && dy == 0 && MathTools.PositiveModulo(tileX, 8) < 5) return RoadMarking.HorizontalLane;
        return RoadMarking.None;
    }

    private TileType SpawnTileAt(int tileX, int tileY)
    {
        var ax = Math.Abs(tileX);
        var ay = Math.Abs(tileY);
        if (ax <= 3 || ay <= 3) return TileType.Asphalt;
        if (ax <= 9 && ay <= 9) return (ax + ay) % 5 == 0 ? TileType.DryGrass : TileType.Pavement;
        return Hashing.Unit(tileX, tileY, _seed + 88) > 0.25f ? TileType.DryGrass : TileType.Dirt;
    }

    private void PopulateResources(Chunk chunk, ISet<string> depletedNodes)
    {
        for (var i = 0; i < 18; i++)
        {
            var localSeed = _seed + i * 104729;
            var lx = Hashing.Range(chunk.Coord.X, chunk.Coord.Y, localSeed, 1, Balance.ChunkSize - 1);
            var ly = Hashing.Range(chunk.Coord.X, chunk.Coord.Y, localSeed + 53, 1, Balance.ChunkSize - 1);
            var wx = chunk.Coord.X * Balance.ChunkSize + lx;
            var wy = chunk.Coord.Y * Balance.ChunkSize + ly;
            var distanceFromSpawn = MathF.Sqrt((float)wx * wx + (float)wy * wy);
            if (distanceFromSpawn < 8f) continue;

            var tile = TileAt(wx, wy);
            if (tile == TileType.Water || tile == TileType.BuildingWall) continue;
            if (chunk.Resources.Any(r => Vector2.DistanceSquared(r.Position, new Vector2((wx + 0.5f) * Balance.TileSize, (wy + 0.5f) * Balance.TileSize)) < 40f * 40f)) continue;

            var roll = Hashing.Unit(wx, wy, _seed + 7000);
            var district = DistrictAt(wx, wy);
            ResourceKind? kind = null;
            if (tile == TileType.DryGrass && roll > (district == District.Wild ? 0.46f : 0.68f)) kind = ResourceKind.Tree;
            else if (tile == TileType.Dirt && roll > 0.83f) kind = ResourceKind.Barrel;
            else if (tile == TileType.Rubble && roll > 0.48f) kind = ResourceKind.ScrapPile;
            else if (tile == TileType.Pavement && roll > 0.74f) kind = roll > 0.90f ? ResourceKind.WreckedCar : ResourceKind.ScrapPile;
            else if (tile == TileType.Asphalt && roll > 0.88f) kind = ResourceKind.WreckedCar;
            else if (tile == TileType.BuildingFloor && roll > 0.54f) kind = BuildingLoot(wx, wy, roll);
            if (kind is null) continue;

            var id = $"{wx}:{wy}:{kind.Value}";
            if (depletedNodes.Contains(id)) continue;
            var pos = new Vector2((wx + 0.5f) * Balance.TileSize, (wy + 0.5f) * Balance.TileSize);
            chunk.Resources.Add(new ResourceNode(id, kind.Value, pos, Durability(kind.Value)));
        }
    }

    private void PopulateDecorations(Chunk chunk)
    {
        for (var i = 0; i < 26; i++)
        {
            var salt = _seed + 50000 + i * 7919;
            var lx = Hashing.Range(chunk.Coord.X, chunk.Coord.Y, salt, 0, Balance.ChunkSize);
            var ly = Hashing.Range(chunk.Coord.X, chunk.Coord.Y, salt + 17, 0, Balance.ChunkSize);
            var wx = chunk.Coord.X * Balance.ChunkSize + lx;
            var wy = chunk.Coord.Y * Balance.ChunkSize + ly;
            if (MathF.Sqrt((float)wx * wx + (float)wy * wy) < 8f) continue;
            if (chunk.Resources.Any(r => Vector2.DistanceSquared(r.Position, new Vector2((wx + 0.5f) * Balance.TileSize, (wy + 0.5f) * Balance.TileSize)) < 34f * 34f)) continue;

            var tile = TileAt(wx, wy);
            if (tile == TileType.Water || tile == TileType.BuildingWall) continue;
            var roll = Hashing.Unit(wx, wy, salt + 99);
            var district = DistrictAt(wx, wy);
            DecorationKind? kind = tile switch
            {
                TileType.DryGrass => roll < 0.32f ? DecorationKind.GrassTuft : roll < 0.42f ? DecorationKind.Bush : roll < 0.48f ? DecorationKind.TireStack : null,
                TileType.Dirt => roll < 0.20f ? DecorationKind.GrassTuft : roll < 0.30f ? DecorationKind.Cardboard : roll < 0.38f ? DecorationKind.BrickDebris : null,
                TileType.Rubble => roll < 0.34f ? DecorationKind.BrickDebris : roll < 0.42f ? DecorationKind.DestroyedWall : roll < 0.50f ? DecorationKind.Cardboard : null,
                TileType.Asphalt => roll < 0.08f ? DecorationKind.Manhole : roll < 0.14f ? DecorationKind.Hydrant : roll < 0.19f ? DecorationKind.GarbageBin : null,
                TileType.Pavement => roll < 0.10f ? DecorationKind.Bench : roll < 0.17f ? DecorationKind.GarbageBin : roll < 0.21f ? DecorationKind.Hydrant : null,
                TileType.BuildingFloor => roll < 0.12f ? DecorationKind.AirVent : roll < 0.18f ? DecorationKind.RoofHole : roll < 0.23f ? DecorationKind.Door : null,
                _ => null
            };
            if (kind is null && district == District.Industrial && roll > 0.92f) kind = DecorationKind.Container;
            if (kind is null) continue;

            var jitterX = (Hashing.Unit(wx, wy, salt + 7) - 0.5f) * Balance.TileSize * 0.35f;
            var jitterY = (Hashing.Unit(wx, wy, salt + 8) - 0.5f) * Balance.TileSize * 0.35f;
            var scale = 1.00f + Hashing.Unit(wx, wy, salt + 9) * 0.34f;
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

    private bool IsSpawnPlaza(int tileX, int tileY) => Math.Abs(tileX) <= 16 && Math.Abs(tileY) <= 16;

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
        if (Math.Abs(tileX) <= 22 && Math.Abs(tileY) <= 22) return default;
        var road = RoadAt(tileX, tileY);
        if (road.NearRoad) return default;
        var district = DistrictAt(tileX, tileY);
        if (district == District.Wild) return default;

        var cellX = MathTools.FloorDiv(tileX, CityCell);
        var cellY = MathTools.FloorDiv(tileY, CityCell);
        var localX = MathTools.PositiveModulo(tileX, CityCell);
        var localY = MathTools.PositiveModulo(tileY, CityCell);
        if (localX < 9 || localX > CityCell - 10 || localY < 9 || localY > CityCell - 10) return default;

        var parcelX = localX < CityCell / 2 ? 0 : 1;
        var parcelY = localY < CityCell / 2 ? 0 : 1;
        var parcelSeedX = cellX * 2 + parcelX;
        var parcelSeedY = cellY * 2 + parcelY;
        var density = district switch { District.City => 0.62f, District.Industrial => 0.48f, District.Ruins => 0.38f, _ => 0.0f };
        if (Hashing.Unit(parcelSeedX, parcelSeedY, _seed + 4100) > density) return default;

        var baseX = parcelX == 0 ? 10 : 35;
        var baseY = parcelY == 0 ? 10 : 35;
        var maxX = parcelX == 0 ? 28 : 54;
        var maxY = parcelY == 0 ? 28 : 54;
        var width = 10 + Hashing.Range(parcelSeedX, parcelSeedY, _seed + 4200, 0, 9);
        var height = 9 + Hashing.Range(parcelSeedX, parcelSeedY, _seed + 4300, 0, 9);
        var offsetX = Hashing.Range(parcelSeedX, parcelSeedY, _seed + 4400, 0, 5);
        var offsetY = Hashing.Range(parcelSeedX, parcelSeedY, _seed + 4500, 0, 5);
        var x0 = baseX + offsetX;
        var y0 = baseY + offsetY;
        var x1 = Math.Min(x0 + width, maxX);
        var y1 = Math.Min(y0 + height, maxY);
        if (localX < x0 || localX > x1 || localY < y0 || localY > y1) return default;

        var ruined = district == District.Ruins || Hashing.Unit(parcelSeedX, parcelSeedY, _seed + 4630) > 0.72f;
        if (ruined && Hashing.Unit(tileX, tileY, _seed + 4640) > 0.93f) return default;

        var border = localX == x0 || localX == x1 || localY == y0 || localY == y1;
        if (!border) return new BuildingInfo(false, true, ruined);

        var doorX = (x0 + x1) / 2;
        var hasDoor = localY == y1 && Math.Abs(localX - doorX) <= 1;
        if (hasDoor) return new BuildingInfo(false, true, ruined);
        if (ruined && Hashing.Unit(tileX, tileY, _seed + 4700) > 0.78f) return new BuildingInfo(false, true, ruined);
        return new BuildingInfo(true, false, ruined);
    }

    private District DistrictAt(int tileX, int tileY)
    {
        if (Math.Abs(tileX) < 72 && Math.Abs(tileY) < 72) return District.City;
        var x = MathTools.FloorDiv(tileX, 96);
        var y = MathTools.FloorDiv(tileY, 96);
        var v = Hashing.Unit(x, y, _seed + 16000);
        if (v < 0.20f) return District.Wild;
        if (v < 0.42f) return District.Ruins;
        if (v < 0.58f) return District.Industrial;
        return District.City;
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

    private enum District { Wild, Ruins, Industrial, City }
    private readonly record struct RoadInfo(bool OnRoad, bool OnSidewalk, bool NearRoad);
    private readonly record struct BuildingInfo(bool IsWall, bool IsFloor, bool Ruined);
}
