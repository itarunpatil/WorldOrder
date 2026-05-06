using Microsoft.Xna.Framework;
using WorldOrder.Core;

namespace WorldOrder.World;

public sealed class ChunkManager
{
    private readonly WorldGenerator _generator;
    private readonly WorldState _state;
    private readonly Dictionary<ChunkCoord, Chunk> _chunks = new();

    public ChunkManager(WorldGenerator generator, WorldState state)
    {
        _generator = generator;
        _state = state;
    }

    public IEnumerable<Chunk> LoadedChunks => _chunks.Values;
    public int LoadedChunkCount => _chunks.Count;

    public void EnsureAround(Vector2 worldPosition)
    {
        var center = ChunkCoord.FromWorld(worldPosition);
        for (var cy = center.Y - Balance.ChunkLoadRadius; cy <= center.Y + Balance.ChunkLoadRadius; cy++)
        {
            for (var cx = center.X - Balance.ChunkLoadRadius; cx <= center.X + Balance.ChunkLoadRadius; cx++)
            {
                var coord = new ChunkCoord(cx, cy);
                if (!_chunks.ContainsKey(coord)) _chunks[coord] = _generator.GenerateChunk(coord, _state.DepletedNodes);
            }
        }

        var remove = new List<ChunkCoord>();
        foreach (var coord in _chunks.Keys)
        {
            if (Math.Abs(coord.X - center.X) > Balance.ChunkKeepRadius || Math.Abs(coord.Y - center.Y) > Balance.ChunkKeepRadius)
            {
                remove.Add(coord);
            }
        }
        foreach (var coord in remove) _chunks.Remove(coord);
    }

    public TileType TileAtWorld(Vector2 world)
    {
        var tx = MathTools.FloorDiv((int)MathF.Floor(world.X), Balance.TileSize);
        var ty = MathTools.FloorDiv((int)MathF.Floor(world.Y), Balance.TileSize);
        return TileAt(tx, ty);
    }

    public TileType TileAt(int tileX, int tileY)
    {
        var coord = new ChunkCoord(MathTools.FloorDiv(tileX, Balance.ChunkSize), MathTools.FloorDiv(tileY, Balance.ChunkSize));
        if (_chunks.TryGetValue(coord, out var chunk))
        {
            var lx = MathTools.PositiveModulo(tileX, Balance.ChunkSize);
            var ly = MathTools.PositiveModulo(tileY, Balance.ChunkSize);
            return chunk.GetLocal(lx, ly);
        }
        return _generator.TileAt(tileX, tileY);
    }

    public bool IsBlocked(Vector2 world)
    {
        var tx = MathTools.FloorDiv((int)MathF.Floor(world.X), Balance.TileSize);
        var ty = MathTools.FloorDiv((int)MathF.Floor(world.Y), Balance.TileSize);
        if (_generator.BlocksMovement(tx, ty)) return true;
        return _state.PlacedBlocks.ContainsKey(BlockKey(tx, ty));
    }

    public bool CanPlaceBlock(int tileX, int tileY)
    {
        var key = BlockKey(tileX, tileY);
        if (_state.PlacedBlocks.ContainsKey(key)) return false;
        var tile = TileAt(tileX, tileY);
        return tile != TileType.Water && tile != TileType.BuildingWall;
    }

    public static string BlockKey(int tileX, int tileY) => $"{tileX}:{tileY}";

    public ResourceNode? FindResource(Vector2 position, float range)
    {
        var rangeSq = range * range;
        ResourceNode? best = null;
        var bestSq = float.MaxValue;
        foreach (var chunk in _chunks.Values)
        {
            foreach (var node in chunk.Resources)
            {
                if (node.IsDestroyed) continue;
                var d = Vector2.DistanceSquared(position, node.Position);
                if (d < rangeSq && d < bestSq)
                {
                    bestSq = d;
                    best = node;
                }
            }
        }
        return best;
    }

    public void MarkDepleted(ResourceNode node)
    {
        _state.DepletedNodes.Add(node.Id);
    }
}
