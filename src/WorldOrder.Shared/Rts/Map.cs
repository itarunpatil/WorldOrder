using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace WorldOrder;

public enum TileKind
{
    Sand,
    Road,
    Rock,
    Water
}

public sealed class MapProp
{
    public string TextureKey { get; init; } = "prop_1";
    public Vector2 Position { get; init; }
    public float Scale { get; init; } = 0.5f;
    public float Rotation { get; init; }
    public bool Blocks { get; init; }
    public float Radius { get; init; } = 48f;
}

public sealed class ResourceNode
{
    public Vector2 Position { get; init; }
    public int Amount { get; set; } = 900;
    public int MaxAmount { get; init; } = 900;
    public float Radius { get; init; } = 120f;
    public bool Depleted => Amount <= 0;
}

public sealed class GameMap
{
    public int Width { get; init; }
    public int Height { get; init; }
    public int TileSize { get; init; } = 96;
    public TileKind[,] Tiles { get; init; } = new TileKind[1, 1];
    public string[,] TextureKeys { get; init; } = new string[1, 1];
    public List<MapProp> Props { get; } = new();
    public List<ResourceNode> ResourceNodes { get; } = new();
    public Vector2 PlayerSpawn { get; init; }
    public List<Vector2> EnemySpawns { get; } = new();
    public List<Vector2> AllySpawns { get; } = new();
    public int PixelWidth => Width * TileSize;
    public int PixelHeight => Height * TileSize;

    public bool IsInsideTile(int x, int y) => x >= 0 && y >= 0 && x < Width && y < Height;

    public Point WorldToTile(Vector2 world)
    {
        return new Point((int)MathF.Floor(world.X / TileSize), (int)MathF.Floor(world.Y / TileSize));
    }

    public Vector2 TileCenter(Point tile)
    {
        return new Vector2((tile.X + 0.5f) * TileSize, (tile.Y + 0.5f) * TileSize);
    }

    public TileKind TileAtWorld(Vector2 world)
    {
        var tile = WorldToTile(world);
        if (!IsInsideTile(tile.X, tile.Y)) return TileKind.Rock;
        return Tiles[tile.X, tile.Y];
    }

    public bool IsPassable(Vector2 world, bool naval)
    {
        var tile = WorldToTile(world);
        if (!IsInsideTile(tile.X, tile.Y)) return false;
        return IsPassableTile(tile.X, tile.Y, naval, world);
    }

    public bool IsPassableTile(int tx, int ty, bool naval)
    {
        if (!IsInsideTile(tx, ty)) return false;
        return IsPassableTile(tx, ty, naval, TileCenter(new Point(tx, ty)));
    }

    private bool IsPassableTile(int tx, int ty, bool naval, Vector2 world)
    {
        var tile = Tiles[tx, ty];
        if (naval) return tile == TileKind.Water;
        if (tile == TileKind.Water || tile == TileKind.Rock) return false;
        foreach (var prop in Props)
        {
            if (!prop.Blocks) continue;
            if (Vector2.DistanceSquared(world, prop.Position) < prop.Radius * prop.Radius) return false;
        }
        return true;
    }

    public List<Vector2> FindPath(Vector2 start, Vector2 goal, bool naval, int maxExpanded = 1200)
    {
        var startTile = WorldToTile(start);
        var goalTile = WorldToTile(goal);
        if (!IsInsideTile(startTile.X, startTile.Y)) return new List<Vector2> { goal };
        if (!IsInsideTile(goalTile.X, goalTile.Y) || !IsPassableTile(goalTile.X, goalTile.Y, naval))
        {
            goalTile = FindNearestPassableTile(goalTile, naval, 12);
        }
        if (!IsInsideTile(goalTile.X, goalTile.Y)) return new List<Vector2> { goal };
        if (startTile == goalTile) return new List<Vector2> { TileCenter(goalTile) };

        var open = new PriorityQueue<Point, float>();
        var cameFrom = new Dictionary<Point, Point>();
        var gScore = new Dictionary<Point, float> { [startTile] = 0f };
        var closed = new HashSet<Point>();
        open.Enqueue(startTile, Heuristic(startTile, goalTile));
        int expanded = 0;

        while (open.Count > 0 && expanded++ < maxExpanded)
        {
            var current = open.Dequeue();
            if (!closed.Add(current)) continue;
            if (current == goalTile) return ReconstructPath(cameFrom, current);

            for (int dy = -1; dy <= 1; dy++)
            for (int dx = -1; dx <= 1; dx++)
            {
                if (dx == 0 && dy == 0) continue;
                var next = new Point(current.X + dx, current.Y + dy);
                if (!IsPassableTile(next.X, next.Y, naval)) continue;
                if (dx != 0 && dy != 0)
                {
                    if (!IsPassableTile(current.X + dx, current.Y, naval) || !IsPassableTile(current.X, current.Y + dy, naval)) continue;
                }
                float moveCost = dx != 0 && dy != 0 ? 1.42f : 1f;
                if (Tiles[next.X, next.Y] == TileKind.Road && !naval) moveCost *= 0.78f;
                float tentative = gScore[current] + moveCost;
                if (!gScore.TryGetValue(next, out float existing) || tentative < existing)
                {
                    cameFrom[next] = current;
                    gScore[next] = tentative;
                    float priority = tentative + Heuristic(next, goalTile);
                    open.Enqueue(next, priority);
                }
            }
        }

        return new List<Vector2> { TileCenter(goalTile) };
    }

    private Point FindNearestPassableTile(Point around, bool naval, int radius)
    {
        for (int r = 1; r <= radius; r++)
        {
            for (int y = around.Y - r; y <= around.Y + r; y++)
            for (int x = around.X - r; x <= around.X + r; x++)
            {
                if (Math.Abs(x - around.X) != r && Math.Abs(y - around.Y) != r) continue;
                if (IsPassableTile(x, y, naval)) return new Point(x, y);
            }
        }
        return new Point(-1, -1);
    }

    private float Heuristic(Point a, Point b)
    {
        return MathF.Abs(a.X - b.X) + MathF.Abs(a.Y - b.Y);
    }

    private List<Vector2> ReconstructPath(Dictionary<Point, Point> cameFrom, Point current)
    {
        var tiles = new List<Point> { current };
        while (cameFrom.TryGetValue(current, out var previous))
        {
            current = previous;
            tiles.Add(current);
        }
        tiles.Reverse();
        var path = new List<Vector2>();
        for (int i = 1; i < tiles.Count; i++)
        {
            if (i < tiles.Count - 1 && i % 2 != 0) continue;
            path.Add(TileCenter(tiles[i]));
        }
        if (path.Count == 0) path.Add(TileCenter(tiles[^1]));
        return path;
    }

    public static GameMap Generate(WorldSettings settings)
    {
        var rand = new Random(settings.Seed);
        const int w = 76;
        const int h = 54;
        var map = new GameMap
        {
            Width = w,
            Height = h,
            Tiles = new TileKind[w, h],
            TextureKeys = new string[w, h],
            PlayerSpawn = new Vector2(12 * 96, h / 2 * 96)
        };

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                SetTile(map, x, y, TileKind.Sand, $"terrain_sand_{rand.Next(0, 4)}");
            }
        }

        DrawRoad(map, new Point(4, h / 2), new Point(w - 6, h / 2 + rand.Next(-4, 5)), 2);
        DrawRoad(map, new Point(w / 2, 5), new Point(w / 2 + rand.Next(-5, 6), h - 6), 2);

        switch (settings.Preset)
        {
            case MapPreset.TwinOasis:
                Lake(map, new Point(w / 2 - 11, h / 2 - 8), 8, 5);
                Lake(map, new Point(w / 2 + 15, h / 2 + 8), 8, 6);
                DrawRoad(map, new Point(12, 8), new Point(w - 14, h - 10), 1);
                break;
            case MapPreset.StonePass:
                Ridge(map, w / 2 - 11, 11, h - 11);
                Ridge(map, w / 2 + 12, 9, h - 13);
                CarvePass(map, w / 2 - 11, h / 2, 7);
                CarvePass(map, w / 2 + 12, h / 2 + 2, 7);
                Lake(map, new Point(w - 18, 10), 6, 5);
                break;
            default:
                Lake(map, new Point(w / 2 + 6, h / 2), 10, 6);
                break;
        }

        for (int cluster = 0; cluster < 14; cluster++)
        {
            int cx = rand.Next(6, w - 6);
            int cy = rand.Next(5, h - 5);
            if (DistanceTiles(cx, cy, 12, h / 2) < 10) continue;
            int radius = rand.Next(1, 3);
            for (int yy = cy - radius; yy <= cy + radius; yy++)
            for (int xx = cx - radius; xx <= cx + radius; xx++)
            {
                if (!map.IsInsideTile(xx, yy)) continue;
                if (map.Tiles[xx, yy] != TileKind.Sand) continue;
                if (rand.NextDouble() < 0.36) SetTile(map, xx, yy, TileKind.Rock, "terrain_rock");
            }
        }

        map.EnemySpawns.Add(new Vector2((w - 8) * 96, 8 * 96));
        map.EnemySpawns.Add(new Vector2((w - 9) * 96, (h - 8) * 96));
        map.EnemySpawns.Add(new Vector2((w / 2 + 13) * 96, 8 * 96));
        map.EnemySpawns.Add(new Vector2((w / 2 + 16) * 96, (h - 8) * 96));
        map.AllySpawns.Add(new Vector2(14 * 96, 8 * 96));
        map.AllySpawns.Add(new Vector2(14 * 96, (h - 8) * 96));
        map.AllySpawns.Add(new Vector2(23 * 96, (h / 2 + 12) * 96));

        ClearSpawnArea(map, map.PlayerSpawn, 5);
        foreach (var p in map.EnemySpawns) ClearSpawnArea(map, p, 4);
        foreach (var p in map.AllySpawns) ClearSpawnArea(map, p, 3);

        AddResourceNodes(map, rand);
        AddProps(map, rand);
        return map;
    }

    private static void SetTile(GameMap map, int x, int y, TileKind kind, string key)
    {
        if (!map.IsInsideTile(x, y)) return;
        map.Tiles[x, y] = kind;
        map.TextureKeys[x, y] = key;
    }

    private static float DistanceTiles(int x0, int y0, int x1, int y1)
    {
        float dx = x0 - x1;
        float dy = y0 - y1;
        return MathF.Sqrt(dx * dx + dy * dy);
    }

    private static void DrawRoad(GameMap map, Point a, Point b, int radius)
    {
        int steps = Math.Max(Math.Abs(a.X - b.X), Math.Abs(a.Y - b.Y));
        for (int i = 0; i <= steps; i++)
        {
            float t = steps == 0 ? 0 : i / (float)steps;
            int cx = (int)MathHelper.Lerp(a.X, b.X, t);
            int cy = (int)MathHelper.Lerp(a.Y, b.Y, t + MathF.Sin(t * MathHelper.Pi * 2.5f) * 0.025f);
            for (int y = cy - radius; y <= cy + radius; y++)
            for (int x = cx - radius; x <= cx + radius; x++)
            {
                if (!map.IsInsideTile(x, y)) continue;
                if (map.Tiles[x, y] != TileKind.Water)
                {
                    SetTile(map, x, y, TileKind.Road, "terrain_road");
                }
            }
        }
    }

    private static void Lake(GameMap map, Point center, int rx, int ry)
    {
        for (int y = center.Y - ry - 1; y <= center.Y + ry + 1; y++)
        for (int x = center.X - rx - 1; x <= center.X + rx + 1; x++)
        {
            if (!map.IsInsideTile(x, y)) continue;
            float dx = (x - center.X) / (float)rx;
            float dy = (y - center.Y) / (float)ry;
            if (dx * dx + dy * dy < 1.0f)
            {
                SetTile(map, x, y, TileKind.Water, "terrain_water");
            }
        }
    }

    private static void Ridge(GameMap map, int x, int y0, int y1)
    {
        for (int y = y0; y <= y1; y++)
        {
            for (int xx = x - 2; xx <= x + 2; xx++)
            {
                SetTile(map, xx, y, TileKind.Rock, "terrain_rock");
            }
        }
    }

    private static void CarvePass(GameMap map, int x, int y, int radius)
    {
        for (int yy = y - radius; yy <= y + radius; yy++)
        for (int xx = x - 3; xx <= x + 3; xx++)
        {
            if (map.IsInsideTile(xx, yy)) SetTile(map, xx, yy, TileKind.Road, "terrain_road");
        }
    }

    private static void ClearSpawnArea(GameMap map, Vector2 spawn, int radius)
    {
        int cx = (int)(spawn.X / map.TileSize);
        int cy = (int)(spawn.Y / map.TileSize);
        for (int y = cy - radius; y <= cy + radius; y++)
        for (int x = cx - radius; x <= cx + radius; x++)
        {
            if (!map.IsInsideTile(x, y)) continue;
            if (map.Tiles[x, y] != TileKind.Water)
            {
                SetTile(map, x, y, TileKind.Sand, $"terrain_sand_{Math.Abs(x * 7 + y * 13) % 4}");
            }
        }
    }

    private static void AddResourceNodes(GameMap map, Random rand)
    {
        var candidateTiles = new[]
        {
            new Point(18, map.Height / 2 - 5),
            new Point(23, map.Height / 2 + 8),
            new Point(map.Width / 2 - 8, 11),
            new Point(map.Width / 2 + 4, map.Height - 12),
            new Point(map.Width - 18, 14),
            new Point(map.Width - 17, map.Height - 13),
            new Point(map.Width / 2 + 10, map.Height / 2 + 1)
        };

        foreach (var c in candidateTiles)
        {
            var tile = c;
            for (int attempts = 0; attempts < 16 && !map.IsPassableTile(tile.X, tile.Y, false); attempts++)
            {
                tile = new Point(Math.Clamp(c.X + rand.Next(-3, 4), 2, map.Width - 3), Math.Clamp(c.Y + rand.Next(-3, 4), 2, map.Height - 3));
            }
            if (!map.IsPassableTile(tile.X, tile.Y, false)) continue;
            var pos = map.TileCenter(tile) + new Vector2(rand.Next(-18, 19), rand.Next(-18, 19));
            map.ResourceNodes.Add(new ResourceNode
            {
                Position = pos,
                Amount = 850 + rand.Next(0, 450),
                MaxAmount = 1200,
                Radius = 118f + rand.Next(-14, 24)
            });
        }
    }

    private static void AddProps(GameMap map, Random rand)
    {
        var propKeys = new[] { "prop_1", "prop_2", "prop_3", "prop_4", "prop_10", "prop_15", "prop_16", "prop_17", "prop_20", "decor_6", "decor_7" };
        int desired = 86;
        int guard = 0;
        while (map.Props.Count < desired && guard++ < desired * 10)
        {
            int tx = rand.Next(2, map.Width - 3);
            int ty = rand.Next(2, map.Height - 3);
            if (map.Tiles[tx, ty] == TileKind.Water || map.Tiles[tx, ty] == TileKind.Road) continue;
            var position = new Vector2((tx + 0.5f) * map.TileSize + rand.Next(-18, 19), (ty + 0.5f) * map.TileSize + rand.Next(-18, 19));
            if (Vector2.Distance(position, map.PlayerSpawn) < 850) continue;
            bool overlapsResource = false;
            foreach (var node in map.ResourceNodes)
            {
                if (Vector2.Distance(position, node.Position) < node.Radius + 72f)
                {
                    overlapsResource = true;
                    break;
                }
            }
            if (overlapsResource) continue;
            bool block = rand.NextDouble() < 0.12;
            map.Props.Add(new MapProp
            {
                TextureKey = propKeys[rand.Next(propKeys.Length)],
                Position = position,
                Scale = (float)(0.18 + rand.NextDouble() * 0.22),
                Rotation = (float)(rand.NextDouble() * MathHelper.TwoPi),
                Blocks = block,
                Radius = block ? rand.Next(38, 72) : 0f
            });
        }
    }
}
