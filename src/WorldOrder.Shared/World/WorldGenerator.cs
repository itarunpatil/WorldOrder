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
    private const int CityCell = 72;
    private const int RoadHalfWidth = 4;
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
        if (IsSpawnSanctuary(tileX, tileY)) return SpawnTileAt(tileX, tileY);

        var road = RoadAt(tileX, tileY);
        if (road.OnRoad) return TileType.Asphalt;
        if (road.OnSidewalk) return TileType.Pavement;

        var building = BuildingAt(tileX, tileY);
        if (building.IsWall) return TileType.BuildingWall;
        if (building.IsFloor) return BuildingFloorTile(tileX, tileY, building);

        var parcel = ParcelAt(tileX, tileY);
        return parcel.Kind switch
        {
            ParcelKind.Park => ParkTile(tileX, tileY, parcel),
            ParcelKind.Plaza => PlazaTile(tileX, tileY, parcel),
            ParcelKind.Parking => ParkingTile(tileX, tileY, parcel),
            ParcelKind.RuinedLot => RuinedLotTile(tileX, tileY, parcel),
            ParcelKind.IndustrialYard => IndustrialTile(tileX, tileY, parcel),
            _ => WildernessTile(tileX, tileY, parcel)
        };
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
            if (MathTools.PositiveModulo(tileY, 9) is 2 or 3) return RoadMarking.CrosswalkHorizontal;
            if (MathTools.PositiveModulo(tileX, 9) is 2 or 3) return RoadMarking.CrosswalkVertical;
            return RoadMarking.None;
        }

        if (onVertical && dx == 0 && MathTools.PositiveModulo(tileY, 10) < 5) return RoadMarking.VerticalLane;
        if (onHorizontal && dy == 0 && MathTools.PositiveModulo(tileX, 10) < 5) return RoadMarking.HorizontalLane;
        return RoadMarking.None;
    }

    private TileType SpawnTileAt(int tileX, int tileY)
    {
        var ax = Math.Abs(tileX);
        var ay = Math.Abs(tileY);
        if (ax <= 4 || ay <= 4) return TileType.Asphalt;
        if (ax <= 12 && ay <= 12) return (ax == 12 || ay == 12 || ax % 5 == 0 || ay % 5 == 0) ? TileType.Pavement : TileType.DryGrass;
        if (ax <= 18 && ay <= 18) return EdgeGrass(tileX, tileY, 0.25f) ? TileType.DryGrass : TileType.Dirt;
        return TileType.Dirt;
    }

    private TileType BuildingFloorTile(int tileX, int tileY, BuildingInfo building)
    {
        if (building.Ruined && Hashing.Unit(tileX, tileY, _seed + 7150) > 0.90f) return TileType.Rubble;
        return TileType.BuildingFloor;
    }

    private TileType ParkTile(int tileX, int tileY, ParcelInfo parcel)
    {
        var lx = tileX - parcel.X0;
        var ly = tileY - parcel.Y0;
        if ((Math.Abs(lx - parcel.Width / 2) <= 1 && MathTools.PositiveModulo(ly, 6) != 0) ||
            (Math.Abs(ly - parcel.Height / 2) <= 1 && MathTools.PositiveModulo(lx, 6) != 0)) return TileType.Dirt;
        if (CoarsePatch(tileX, tileY, 7, _seed + 2900) > 0.82f) return TileType.Dirt;
        return TileType.DryGrass;
    }

    private TileType PlazaTile(int tileX, int tileY, ParcelInfo parcel)
    {
        var lx = tileX - parcel.X0;
        var ly = tileY - parcel.Y0;
        if (lx < 2 || ly < 2 || lx > parcel.Width - 3 || ly > parcel.Height - 3) return TileType.Pavement;
        if (Hashing.Unit(MathTools.FloorDiv(tileX, 6), MathTools.FloorDiv(tileY, 6), _seed + 5100) > 0.88f) return TileType.DryGrass;
        return TileType.Pavement;
    }

    private TileType ParkingTile(int tileX, int tileY, ParcelInfo parcel)
    {
        var lx = tileX - parcel.X0;
        var ly = tileY - parcel.Y0;
        if (lx < 2 || ly < 2 || lx > parcel.Width - 3 || ly > parcel.Height - 3) return TileType.Pavement;
        if (Hashing.Unit(tileX, tileY, _seed + 5220) > 0.965f) return TileType.Rubble;
        return TileType.Asphalt;
    }

    private TileType RuinedLotTile(int tileX, int tileY, ParcelInfo parcel)
    {
        var rough = CoarsePatch(tileX, tileY, 8, _seed + 5400);
        var large = CoarsePatch(tileX, tileY, 19, _seed + 5410);
        if (rough > 0.73f || large > 0.82f) return TileType.Rubble;
        if (large < 0.25f) return TileType.DryGrass;
        return TileType.Dirt;
    }

    private TileType IndustrialTile(int tileX, int tileY, ParcelInfo parcel)
    {
        var lx = tileX - parcel.X0;
        var ly = tileY - parcel.Y0;
        if (lx < 2 || ly < 2 || lx > parcel.Width - 3 || ly > parcel.Height - 3) return TileType.Pavement;
        var patch = CoarsePatch(tileX, tileY, 10, _seed + 5600);
        if (patch > 0.80f) return TileType.Rubble;
        if (patch > 0.47f) return TileType.Pavement;
        return TileType.Dirt;
    }

    private TileType WildernessTile(int tileX, int tileY, ParcelInfo parcel)
    {
        var water = CoarsePatch(tileX, tileY, 34, _seed + 1600);
        if (parcel.District == District.Wild && water < 0.055f && Math.Abs(tileX) + Math.Abs(tileY) > 120) return TileType.Water;
        var dirt = CoarsePatch(tileX, tileY, 18, _seed + 1700);
        var rubble = CoarsePatch(tileX, tileY, 9, _seed + 1710);
        if (parcel.District == District.Ruins && rubble > 0.86f) return TileType.Rubble;
        return dirt > 0.43f ? TileType.DryGrass : TileType.Dirt;
    }

    private bool EdgeGrass(int tileX, int tileY, float threshold) => CoarsePatch(tileX, tileY, 12, _seed + 1800) > threshold;

    private void PopulateResources(Chunk chunk, ISet<string> depletedNodes)
    {
        for (var i = 0; i < 14; i++)
        {
            var salt = _seed + i * 104729;
            var lx = Hashing.Range(chunk.Coord.X, chunk.Coord.Y, salt, 1, Balance.ChunkSize - 1);
            var ly = Hashing.Range(chunk.Coord.X, chunk.Coord.Y, salt + 53, 1, Balance.ChunkSize - 1);
            var wx = chunk.Coord.X * Balance.ChunkSize + lx;
            var wy = chunk.Coord.Y * Balance.ChunkSize + ly;
            var distanceFromSpawn = MathF.Sqrt((float)wx * wx + (float)wy * wy);
            if (distanceFromSpawn < 12f) continue;

            var tile = TileAt(wx, wy);
            if (tile == TileType.Water || tile == TileType.BuildingWall) continue;
            if (chunk.Resources.Any(r => Vector2.DistanceSquared(r.Position, TileCenter(wx, wy)) < 48f * 48f)) continue;

            var roll = Hashing.Unit(wx, wy, salt + 7000);
            var parcel = ParcelAt(wx, wy);
            ResourceKind? kind = null;
            if (tile == TileType.DryGrass && roll > (parcel.Kind == ParcelKind.Park || parcel.District == District.Wild ? 0.44f : 0.76f)) kind = ResourceKind.Tree;
            else if (tile == TileType.Rubble && roll > 0.45f) kind = ResourceKind.ScrapPile;
            else if (tile == TileType.Asphalt && parcel.Kind == ParcelKind.Parking && roll > 0.58f) kind = ResourceKind.WreckedCar;
            else if (tile == TileType.Asphalt && roll > 0.89f) kind = ResourceKind.WreckedCar;
            else if (tile == TileType.Pavement && roll > 0.86f) kind = ResourceKind.ScrapPile;
            else if (tile == TileType.Dirt && roll > 0.86f) kind = ResourceKind.Barrel;
            else if (tile == TileType.BuildingFloor && roll > 0.58f) kind = BuildingLoot(wx, wy, roll);
            if (kind is null) continue;

            var id = $"{wx}:{wy}:{kind.Value}";
            if (depletedNodes.Contains(id)) continue;
            chunk.Resources.Add(new ResourceNode(id, kind.Value, TileCenter(wx, wy), Durability(kind.Value)));
        }
    }

    private void PopulateDecorations(Chunk chunk)
    {
        for (var i = 0; i < 20; i++)
        {
            var salt = _seed + 50000 + i * 7919;
            var lx = Hashing.Range(chunk.Coord.X, chunk.Coord.Y, salt, 0, Balance.ChunkSize);
            var ly = Hashing.Range(chunk.Coord.X, chunk.Coord.Y, salt + 17, 0, Balance.ChunkSize);
            var wx = chunk.Coord.X * Balance.ChunkSize + lx;
            var wy = chunk.Coord.Y * Balance.ChunkSize + ly;
            if (MathF.Sqrt((float)wx * wx + (float)wy * wy) < 10f) continue;
            if (chunk.Resources.Any(r => Vector2.DistanceSquared(r.Position, TileCenter(wx, wy)) < 38f * 38f)) continue;

            var tile = TileAt(wx, wy);
            if (tile == TileType.Water || tile == TileType.BuildingWall) continue;
            var roll = Hashing.Unit(wx, wy, salt + 99);
            var parcel = ParcelAt(wx, wy);
            DecorationKind? kind = tile switch
            {
                TileType.DryGrass => roll < 0.24f ? DecorationKind.GrassTuft : roll < 0.34f ? DecorationKind.Bush : roll < 0.39f ? DecorationKind.TireStack : null,
                TileType.Dirt => roll < 0.13f ? DecorationKind.GrassTuft : roll < 0.21f ? DecorationKind.Cardboard : roll < 0.27f ? DecorationKind.BrickDebris : null,
                TileType.Rubble => roll < 0.24f ? DecorationKind.BrickDebris : roll < 0.34f ? DecorationKind.DestroyedWall : roll < 0.41f ? DecorationKind.Cardboard : null,
                TileType.Asphalt => roll < 0.055f ? DecorationKind.Manhole : roll < 0.085f ? DecorationKind.Hydrant : roll < 0.13f ? DecorationKind.GarbageBin : null,
                TileType.Pavement => roll < 0.06f ? DecorationKind.Bench : roll < 0.11f ? DecorationKind.GarbageBin : roll < 0.15f ? DecorationKind.Hydrant : null,
                TileType.BuildingFloor => roll < 0.09f ? DecorationKind.AirVent : roll < 0.14f ? DecorationKind.RoofHole : roll < 0.19f ? DecorationKind.Door : null,
                _ => null
            };
            if (kind is null && parcel.Kind == ParcelKind.IndustrialYard && roll > 0.90f) kind = DecorationKind.Container;
            if (kind is null) continue;

            var jitterX = (Hashing.Unit(wx, wy, salt + 7) - 0.5f) * Balance.TileSize * 0.30f;
            var jitterY = (Hashing.Unit(wx, wy, salt + 8) - 0.5f) * Balance.TileSize * 0.30f;
            var scale = 0.95f + Hashing.Unit(wx, wy, salt + 9) * 0.30f;
            var id = $"d:{wx}:{wy}:{kind.Value}:{i}";
            chunk.Decorations.Add(new DecorationNode(id, kind.Value, TileCenter(wx, wy) + new Vector2(jitterX, jitterY), scale));
        }
    }

    private ResourceKind BuildingLoot(int tileX, int tileY, float roll)
    {
        var room = Hashing.Unit(MathTools.FloorDiv(tileX, 5), MathTools.FloorDiv(tileY, 5), _seed + 8100);
        if (roll > 0.92f) return ResourceKind.AmmoCache;
        if (room > 0.80f) return ResourceKind.MedicalCache;
        if (room > 0.47f) return ResourceKind.FoodCache;
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

    private static Vector2 TileCenter(int tileX, int tileY) => new((tileX + 0.5f) * Balance.TileSize, (tileY + 0.5f) * Balance.TileSize);

    private bool IsSpawnSanctuary(int tileX, int tileY) => Math.Abs(tileX) <= 22 && Math.Abs(tileY) <= 22;

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
        if (Math.Abs(tileX) <= 28 && Math.Abs(tileY) <= 28) return default;
        var road = RoadAt(tileX, tileY);
        if (road.NearRoad) return default;
        var parcel = ParcelAt(tileX, tileY);
        if (parcel.Kind is ParcelKind.Park or ParcelKind.Plaza or ParcelKind.Parking or ParcelKind.Wilderness) return default;
        if (tileX < parcel.X0 + 3 || tileY < parcel.Y0 + 3 || tileX > parcel.X1 - 3 || tileY > parcel.Y1 - 3) return default;

        var density = parcel.Kind switch
        {
            ParcelKind.IndustrialYard => 0.34f,
            ParcelKind.RuinedLot => 0.48f,
            _ => parcel.District == District.City ? 0.70f : 0.46f
        };
        if (Hashing.Unit(parcel.KeyX, parcel.KeyY, _seed + 4100) > density) return default;

        var marginX = 4 + Hashing.Range(parcel.KeyX, parcel.KeyY, _seed + 4200, 0, 5);
        var marginY = 4 + Hashing.Range(parcel.KeyX, parcel.KeyY, _seed + 4210, 0, 5);
        var maxWidth = Math.Max(10, parcel.Width - 6);
        var maxHeight = Math.Max(9, parcel.Height - 6);
        var width = Math.Clamp(parcel.Width - marginX * 2 - Hashing.Range(parcel.KeyX, parcel.KeyY, _seed + 4300, 0, 8), 10, maxWidth);
        var height = Math.Clamp(parcel.Height - marginY * 2 - Hashing.Range(parcel.KeyX, parcel.KeyY, _seed + 4310, 0, 8), 9, maxHeight);
        var x0 = parcel.X0 + marginX;
        var y0 = parcel.Y0 + marginY;
        var x1 = Math.Min(x0 + width, parcel.X1 - 3);
        var y1 = Math.Min(y0 + height, parcel.Y1 - 3);
        if (tileX < x0 || tileX > x1 || tileY < y0 || tileY > y1) return default;

        var ruined = parcel.Kind == ParcelKind.RuinedLot || Hashing.Unit(parcel.KeyX, parcel.KeyY, _seed + 4630) > 0.70f;
        if (ruined && Hashing.Unit(tileX, tileY, _seed + 4640) > 0.94f) return default;

        var border = tileX == x0 || tileX == x1 || tileY == y0 || tileY == y1;
        if (!border) return new BuildingInfo(false, true, ruined);

        var doorX = Math.Clamp(x0 + 2 + Hashing.Range(parcel.KeyX, parcel.KeyY, _seed + 4650, 0, Math.Max(1, width - 3)), x0 + 2, x1 - 2);
        var door = tileY == y1 && Math.Abs(tileX - doorX) <= 1;
        if (door) return new BuildingInfo(false, true, ruined);
        if (ruined && Hashing.Unit(tileX, tileY, _seed + 4700) > 0.74f) return new BuildingInfo(false, true, ruined);
        return new BuildingInfo(true, false, ruined);
    }

    private ParcelInfo ParcelAt(int tileX, int tileY)
    {
        var cellX = MathTools.FloorDiv(tileX, CityCell);
        var cellY = MathTools.FloorDiv(tileY, CityCell);
        var localX = MathTools.PositiveModulo(tileX, CityCell);
        var localY = MathTools.PositiveModulo(tileY, CityCell);
        var left = localX < CityCell / 2;
        var top = localY < CityCell / 2;
        var px = cellX * 2 + (left ? 0 : 1);
        var py = cellY * 2 + (top ? 0 : 1);
        var x0 = cellX * CityCell + (left ? RoadHalfWidth + SidewalkWidth + 2 : CityCell / 2 + 2);
        var y0 = cellY * CityCell + (top ? RoadHalfWidth + SidewalkWidth + 2 : CityCell / 2 + 2);
        var x1 = cellX * CityCell + (left ? CityCell / 2 - 2 : CityCell - RoadHalfWidth - SidewalkWidth - 2);
        var y1 = cellY * CityCell + (top ? CityCell / 2 - 2 : CityCell - RoadHalfWidth - SidewalkWidth - 2);
        if (x1 <= x0) x1 = x0 + 8;
        if (y1 <= y0) y1 = y0 + 8;
        var district = DistrictAt(cellX, cellY);
        var roll = Hashing.Unit(px, py, _seed + 3000);
        var kind = district switch
        {
            District.Wild => roll < 0.72f ? ParcelKind.Wilderness : roll < 0.88f ? ParcelKind.Park : ParcelKind.RuinedLot,
            District.Ruins => roll < 0.38f ? ParcelKind.RuinedLot : roll < 0.56f ? ParcelKind.Park : roll < 0.72f ? ParcelKind.Parking : ParcelKind.Block,
            District.Industrial => roll < 0.46f ? ParcelKind.IndustrialYard : roll < 0.64f ? ParcelKind.Parking : roll < 0.78f ? ParcelKind.RuinedLot : ParcelKind.Block,
            _ => roll < 0.18f ? ParcelKind.Park : roll < 0.30f ? ParcelKind.Plaza : roll < 0.44f ? ParcelKind.Parking : roll < 0.58f ? ParcelKind.RuinedLot : ParcelKind.Block
        };
        return new ParcelInfo(px, py, x0, y0, x1, y1, district, kind);
    }

    private District DistrictAt(int cellX, int cellY)
    {
        if (Math.Abs(cellX) <= 1 && Math.Abs(cellY) <= 1) return District.City;
        var v = Hashing.Unit(cellX, cellY, _seed + 16000);
        if (v < 0.18f) return District.Wild;
        if (v < 0.42f) return District.Ruins;
        if (v < 0.59f) return District.Industrial;
        return District.City;
    }

    private float CoarsePatch(int x, int y, int scale, int seed)
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
        return MathHelper.Lerp(MathHelper.Lerp(a, b, tx), MathHelper.Lerp(c, d, tx), ty);
    }

    private enum District { Wild, Ruins, Industrial, City }
    private enum ParcelKind { Wilderness, Park, Plaza, Parking, RuinedLot, IndustrialYard, Block }
    private readonly record struct RoadInfo(bool OnRoad, bool OnSidewalk, bool NearRoad);
    private readonly record struct BuildingInfo(bool IsWall, bool IsFloor, bool Ruined);
    private readonly record struct ParcelInfo(int KeyX, int KeyY, int X0, int Y0, int X1, int Y1, District District, ParcelKind Kind)
    {
        public int Width => X1 - X0 + 1;
        public int Height => Y1 - Y0 + 1;
    }
}
