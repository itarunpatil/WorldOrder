using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

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

public sealed class GameMap
{
    public int Width { get; init; }
    public int Height { get; init; }
    public int TileSize { get; init; } = 96;
    public TileKind[,] Tiles { get; init; } = new TileKind[1, 1];
    public string[,] TextureKeys { get; init; } = new string[1, 1];
    public List<MapProp> Props { get; } = new();
    public Vector2 PlayerSpawn { get; init; }
    public List<Vector2> EnemySpawns { get; } = new();
    public List<Vector2> AllySpawns { get; } = new();
    public int PixelWidth => Width * TileSize;
    public int PixelHeight => Height * TileSize;

    public bool IsInsideTile(int x, int y) => x >= 0 && y >= 0 && x < Width && y < Height;

    public TileKind TileAtWorld(Vector2 world)
    {
        int tx = (int)MathF.Floor(world.X / TileSize);
        int ty = (int)MathF.Floor(world.Y / TileSize);
        if (!IsInsideTile(tx, ty)) return TileKind.Rock;
        return Tiles[tx, ty];
    }

    public bool IsPassable(Vector2 world, bool naval)
    {
        int tx = (int)MathF.Floor(world.X / TileSize);
        int ty = (int)MathF.Floor(world.Y / TileSize);
        if (!IsInsideTile(tx, ty)) return false;
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

    public static GameMap Generate(WorldSettings settings)
    {
        var rand = new Random(settings.Seed);
        int w = 76;
        int h = 54;
        var map = new GameMap
        {
            Width = w,
            Height = h,
            Tiles = new TileKind[w, h],
            TextureKeys = new string[w, h],
            PlayerSpawn = new Vector2(9 * 96, h / 2 * 96)
        };

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                map.Tiles[x, y] = TileKind.Sand;
                int variant = 1 + rand.Next(1, 6);
                map.TextureKeys[x, y] = "tile_" + variant;
            }
        }

        DrawRoad(map, new Point(3, h / 2), new Point(w - 5, h / 2 + rand.Next(-5, 6)), 2);
        DrawRoad(map, new Point(w / 2, 4), new Point(w / 2 + rand.Next(-5, 5), h - 5), 2);

        switch (settings.Preset)
        {
            case MapPreset.TwinOasis:
                Lake(map, new Point(w / 2 - 10, h / 2 - 8), 7, 5);
                Lake(map, new Point(w / 2 + 14, h / 2 + 8), 8, 6);
                break;
            case MapPreset.StonePass:
                Ridge(map, w / 2 - 8, 10, h - 10);
                Ridge(map, w / 2 + 9, 7, h - 12);
                Lake(map, new Point(w - 18, 10), 6, 5);
                break;
            default:
                Lake(map, new Point(w / 2 + 5, h / 2), 10, 6);
                break;
        }

        for (int i = 0; i < 135; i++)
        {
            int x = rand.Next(1, w - 2);
            int y = rand.Next(1, h - 2);
            if (map.Tiles[x, y] != TileKind.Sand) continue;
            if (Vector2.Distance(new Vector2(x * 96, y * 96), map.PlayerSpawn) < 850) continue;
            if (rand.NextDouble() < 0.38)
            {
                map.Tiles[x, y] = TileKind.Rock;
                map.TextureKeys[x, y] = "tile_" + (7 + rand.Next(0, 5));
            }
        }

        var propKeys = new[] { "prop_1", "prop_2", "prop_3", "prop_4", "prop_10", "prop_15", "prop_16", "prop_17", "prop_20", "decor_6", "decor_7" };
        for (int i = 0; i < 190; i++)
        {
            int tx = rand.Next(2, w - 3);
            int ty = rand.Next(2, h - 3);
            if (map.Tiles[tx, ty] == TileKind.Water || map.Tiles[tx, ty] == TileKind.Road) continue;
            bool block = rand.NextDouble() < 0.34;
            map.Props.Add(new MapProp
            {
                TextureKey = propKeys[rand.Next(propKeys.Length)],
                Position = new Vector2((tx + 0.5f) * map.TileSize + rand.Next(-18, 19), (ty + 0.5f) * map.TileSize + rand.Next(-18, 19)),
                Scale = (float)(0.22 + rand.NextDouble() * 0.24),
                Rotation = (float)(rand.NextDouble() * MathHelper.TwoPi),
                Blocks = block,
                Radius = block ? rand.Next(42, 86) : 0f
            });
        }

        map.EnemySpawns.Add(new Vector2((w - 8) * 96, 8 * 96));
        map.EnemySpawns.Add(new Vector2((w - 9) * 96, (h - 8) * 96));
        map.EnemySpawns.Add(new Vector2((w / 2 + 12) * 96, 7 * 96));
        map.EnemySpawns.Add(new Vector2((w / 2 + 16) * 96, (h - 8) * 96));
        map.AllySpawns.Add(new Vector2(13 * 96, 8 * 96));
        map.AllySpawns.Add(new Vector2(13 * 96, (h - 8) * 96));
        map.AllySpawns.Add(new Vector2(23 * 96, (h / 2 + 12) * 96));
        return map;
    }

    private static void DrawRoad(GameMap map, Point a, Point b, int radius)
    {
        int steps = Math.Max(Math.Abs(a.X - b.X), Math.Abs(a.Y - b.Y));
        for (int i = 0; i <= steps; i++)
        {
            float t = steps == 0 ? 0 : i / (float)steps;
            int cx = (int)MathHelper.Lerp(a.X, b.X, t);
            int cy = (int)MathHelper.Lerp(a.Y, b.Y, t + MathF.Sin(t * MathHelper.Pi * 3) * 0.03f);
            for (int y = cy - radius; y <= cy + radius; y++)
            for (int x = cx - radius; x <= cx + radius; x++)
            {
                if (!map.IsInsideTile(x, y)) continue;
                map.Tiles[x, y] = TileKind.Road;
                map.TextureKeys[x, y] = "road_18";
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
                map.Tiles[x, y] = TileKind.Water;
                map.TextureKeys[x, y] = "lake";
            }
        }
    }

    private static void Ridge(GameMap map, int x, int y0, int y1)
    {
        for (int y = y0; y <= y1; y++)
        {
            for (int xx = x - 2; xx <= x + 2; xx++)
            {
                if (!map.IsInsideTile(xx, y)) continue;
                map.Tiles[xx, y] = TileKind.Rock;
                map.TextureKeys[xx, y] = "tile_8";
            }
        }
    }
}
