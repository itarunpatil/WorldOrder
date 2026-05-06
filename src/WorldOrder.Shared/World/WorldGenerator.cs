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
    private readonly int _seed;
    private readonly WorldMapDefinition _map;

    public WorldGenerator(int seed, string mapId = WorldMapCatalog.DefaultMapId)
    {
        _seed = seed;
        _map = WorldMapCatalog.Create(mapId);
    }

    public string MapId => _map.Id;
    public string MapName => _map.Name;
    public Rectangle MapBoundsTiles => new(_map.MinTileX, _map.MinTileY, _map.Width, _map.Height);

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

        var chunkRect = new Rectangle(coord.X * Balance.ChunkSize, coord.Y * Balance.ChunkSize, Balance.ChunkSize, Balance.ChunkSize);
        foreach (var r in _map.Resources)
        {
            var tileX = MathTools.FloorDiv((int)MathF.Floor(r.Position.X), Balance.TileSize);
            var tileY = MathTools.FloorDiv((int)MathF.Floor(r.Position.Y), Balance.TileSize);
            if (!chunkRect.Contains(tileX, tileY) || depletedNodes.Contains(r.Id)) continue;
            chunk.Resources.Add(new ResourceNode(r.Id, r.Kind, r.Position, r.Durability));
        }

        foreach (var d in _map.Decorations)
        {
            var tileX = MathTools.FloorDiv((int)MathF.Floor(d.Position.X), Balance.TileSize);
            var tileY = MathTools.FloorDiv((int)MathF.Floor(d.Position.Y), Balance.TileSize);
            if (!chunkRect.Contains(tileX, tileY)) continue;
            chunk.Decorations.Add(new DecorationNode(d.Id, d.Kind, d.Position, d.Scale, d.BlocksMovement));
        }
        return chunk;
    }

    public TileType TileAt(int tileX, int tileY) => _map.TileAt(tileX, tileY);

    public bool BlocksMovement(int tileX, int tileY)
    {
        if (!_map.ContainsTile(tileX, tileY)) return true;
        var tile = TileAt(tileX, tileY);
        return tile == TileType.BuildingWall || tile == TileType.Water;
    }

    public RoadMarking RoadMarkingAt(int tileX, int tileY)
    {
        if (TileAt(tileX, tileY) != TileType.Asphalt) return RoadMarking.None;
        var left = TileAt(tileX - 1, tileY) == TileType.Asphalt;
        var right = TileAt(tileX + 1, tileY) == TileType.Asphalt;
        var up = TileAt(tileX, tileY - 1) == TileType.Asphalt;
        var down = TileAt(tileX, tileY + 1) == TileType.Asphalt;
        var horizontal = left && right;
        var vertical = up && down;
        if (horizontal && vertical)
        {
            if (MathTools.PositiveModulo(tileX + _seed, 9) is 0 or 1) return RoadMarking.CrosswalkVertical;
            if (MathTools.PositiveModulo(tileY + _seed, 9) is 0 or 1) return RoadMarking.CrosswalkHorizontal;
            return RoadMarking.None;
        }
        if (horizontal && MathTools.PositiveModulo(tileX + 3, 10) < 4) return RoadMarking.HorizontalLane;
        if (vertical && MathTools.PositiveModulo(tileY + 3, 10) < 4) return RoadMarking.VerticalLane;
        return RoadMarking.None;
    }

    public Vector2 NextZombieSpawn(Vector2 playerPosition, int salt)
    {
        if (_map.ZombieSpawnPoints.Count == 0) return playerPosition + new Vector2(720f, 0f);
        var best = _map.ZombieSpawnPoints[0];
        var bestScore = float.MinValue;
        for (var i = 0; i < _map.ZombieSpawnPoints.Count; i++)
        {
            var p = _map.ZombieSpawnPoints[i];
            var distSq = Vector2.DistanceSquared(p, playerPosition);
            var score = distSq + Hashing.Unit(salt, i, _seed + 9123) * 80000f;
            if (score > bestScore)
            {
                bestScore = score;
                best = p;
            }
        }
        return best;
    }
}
