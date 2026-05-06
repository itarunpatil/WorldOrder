using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WorldOrder.Assets;
using WorldOrder.Core;
using WorldOrder.Entities;
using WorldOrder.Gameplay;
using WorldOrder.World;

namespace WorldOrder.Rendering;

public sealed class WorldRenderer
{
    private readonly GameRoot _game;

    public WorldRenderer(GameRoot game) => _game = game;

    public void DrawWorld(SpriteBatch batch, WorldSession session, GameTime gameTime)
    {
        var bounds = _game.Camera.GetWorldBounds(_game.GraphicsDevice);
        var minTileX = MathTools.FloorDiv(bounds.Left, Balance.TileSize) - 1;
        var maxTileX = MathTools.FloorDiv(bounds.Right, Balance.TileSize) + 1;
        var minTileY = MathTools.FloorDiv(bounds.Top, Balance.TileSize) - 1;
        var maxTileY = MathTools.FloorDiv(bounds.Bottom, Balance.TileSize) + 1;

        for (var ty = minTileY; ty <= maxTileY; ty++)
        {
            for (var tx = minTileX; tx <= maxTileX; tx++)
            {
                var tile = session.Chunks.TileAt(tx, ty);
                var dest = new Rectangle(tx * Balance.TileSize, ty * Balance.TileSize, Balance.TileSize, Balance.TileSize);
                batch.Draw(_game.Art.Tile(tile, tx, ty), dest, Color.White);
            }
        }

        DrawGroundEffects(batch, session, bounds);
        foreach (var chunk in session.Chunks.LoadedChunks)
        {
            if (!chunk.WorldRectangle.Intersects(bounds)) continue;
            foreach (var decoration in chunk.Decorations.OrderBy(d => d.Position.Y)) DrawDecoration(batch, decoration, bounds);
            foreach (var node in chunk.Resources)
            {
                if (node.IsDestroyed) continue;
                DrawResource(batch, node);
            }
        }

        foreach (var pair in session.State.PlacedBlocks)
        {
            var parts = pair.Key.Split(':');
            if (parts.Length != 2 || !int.TryParse(parts[0], out var tx) || !int.TryParse(parts[1], out var ty)) continue;
            var dest = new Rectangle(tx * Balance.TileSize, ty * Balance.TileSize, Balance.TileSize, Balance.TileSize);
            if (!dest.Intersects(bounds)) continue;
            if (pair.Value.Kind == BlockKind.Floor)
            {
                batch.Draw(_game.Art.Tile(TileType.BuildingFloor, tx, ty), dest, Color.White);
                continue;
            }
            var texture = pair.Value.Kind switch
            {
                BlockKind.ReinforcedWall => _game.Art.Texture("wallreinforced"),
                BlockKind.Campfire => _game.Art.Texture("campfire"),
                _ => _game.Art.Texture("wallwood")
            };
            batch.Draw(texture, dest, Color.White);
        }

        foreach (var entity in session.Entities.All.OrderBy(e => e.Position.Y))
        {
            DrawEntity(batch, entity, session, gameTime);
        }
        DrawPlayer(batch, session, gameTime);
        DrawForegroundEffects(batch, session);
        DrawNightOverlay(batch, session);
    }

    private void DrawDecoration(SpriteBatch batch, DecorationNode decoration, Rectangle bounds)
    {
        var texture = decoration.Kind switch
        {
            DecorationKind.GrassTuft => _game.Art.Texture(decoration.Id.GetHashCode() % 2 == 0 ? "grass1" : "grass2"),
            DecorationKind.Bush => _game.Art.Texture("bush"),
            DecorationKind.TireStack => _game.Art.Texture("tires"),
            DecorationKind.Cardboard => _game.Art.Texture("cardboard"),
            DecorationKind.GarbageBin => _game.Art.Texture("garbagebin"),
            DecorationKind.Hydrant => _game.Art.Texture("hydrant"),
            DecorationKind.Manhole => _game.Art.Texture("manhole"),
            DecorationKind.Bench => _game.Art.Texture("bench"),
            DecorationKind.Container => _game.Art.Texture("container"),
            DecorationKind.AirVent => _game.Art.Texture("airvent"),
            DecorationKind.Door => _game.Art.Texture("door"),
            DecorationKind.DestroyedWall => _game.Art.Texture("destroyedwall"),
            DecorationKind.BrickDebris => _game.Art.Texture("brickdebris"),
            DecorationKind.RoofHole => _game.Art.Texture("roofhole"),
            DecorationKind.Fence => _game.Art.Texture("fence"),
            _ => _game.Art.Texture("crate")
        };
        var scale = decoration.Kind switch
        {
            DecorationKind.Container => decoration.Scale * 0.82f,
            DecorationKind.Bench => decoration.Scale * 1.25f,
            DecorationKind.Manhole => decoration.Scale * 1.45f,
            DecorationKind.GrassTuft => decoration.Scale * 0.95f,
            _ => decoration.Scale
        };
        var dest = new Rectangle((int)(decoration.Position.X - texture.Width * scale * 0.5f), (int)(decoration.Position.Y - texture.Height * scale * 0.72f), (int)(texture.Width * scale), (int)(texture.Height * scale));
        if (!dest.Intersects(bounds)) return;
        batch.Draw(texture, dest, Color.White);
    }

    private void DrawResource(SpriteBatch batch, ResourceNode node)
    {
        var texture = node.Kind switch
        {
            ResourceKind.Tree => _game.Art.Texture("tree"),
            ResourceKind.WreckedCar => _game.Art.Texture("car"),
            ResourceKind.Barrel => _game.Art.Texture("barrel"),
            ResourceKind.FoodCache => _game.Art.Texture("food"),
            ResourceKind.WaterCache => _game.Art.Texture("water"),
            ResourceKind.MedicalCache => _game.Art.Texture("medkit"),
            ResourceKind.AmmoCache => _game.Art.Texture("crate"),
            _ => _game.Art.Texture("crate")
        };
        var scale = node.Kind == ResourceKind.Tree ? 1.55f : node.Kind == ResourceKind.WreckedCar ? 1.35f : 1.25f;
        var dest = new Rectangle((int)(node.Position.X - texture.Width * scale * 0.5f), (int)(node.Position.Y - texture.Height * scale * 0.72f), (int)(texture.Width * scale), (int)(texture.Height * scale));
        batch.Draw(texture, dest, Color.White);
        if (node.Durability < node.MaxDurability)
        {
            DrawWorldBar(batch, new Vector2(node.Position.X - 16, node.Position.Y - 32), 32, node.Durability / (float)node.MaxDurability, new Color(218, 174, 77));
        }
    }

    private void DrawEntity(SpriteBatch batch, Entity entity, WorldSession session, GameTime gameTime)
    {
        switch (entity)
        {
            case Zombie zombie:
                DrawZombie(batch, zombie, gameTime);
                break;
            case Pickup pickup:
                DrawPickup(batch, pickup);
                break;
        }
    }

    private void DrawPlayer(SpriteBatch batch, WorldSession session, GameTime gameTime)
    {
        var player = session.Player;
        var id = SpriteId.PlayerIdleDown;
        if (Math.Abs(player.Facing.X) > Math.Abs(player.Facing.Y)) id = player.IsMoving ? SpriteId.PlayerRunSide : SpriteId.PlayerIdleSide;
        else if (player.Facing.Y < 0f) id = player.IsMoving ? SpriteId.PlayerRunUp : SpriteId.PlayerIdleUp;
        else id = player.IsMoving ? SpriteId.PlayerRunDown : SpriteId.PlayerIdleDown;
        var flip = player.Facing.X < 0f;
        if (_game.Art.TrySheet(id, out var sheet))
        {
            var scale = 2.3f;
            var source = sheet.SourceAt((float)gameTime.TotalGameTime.TotalSeconds);
            var origin = new Vector2(sheet.FrameWidth * 0.5f, sheet.FrameHeight * 0.82f);
            batch.Draw(sheet.Texture, player.Position, source, Color.White, 0f, origin, scale, flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);
        }
        else
        {
            var texture = _game.Art.Texture("player");
            batch.Draw(texture, new Rectangle((int)player.Position.X - 18, (int)player.Position.Y - 32, 36, 44), Color.White);
        }
    }

    private void DrawZombie(SpriteBatch batch, Zombie zombie, GameTime gameTime)
    {
        var id = SpriteId.ZombieWalkDown;
        if (Math.Abs(zombie.Facing.X) > Math.Abs(zombie.Facing.Y)) id = SpriteId.ZombieWalkSide;
        else if (zombie.Facing.Y < 0f) id = SpriteId.ZombieWalkUp;
        var flip = zombie.Facing.X < 0f;
        var tint = zombie.Tier == ZombieTier.Brute ? new Color(205, 205, 205) : Color.White;
        if (zombie.HurtTimer > 0f) tint = Color.Lerp(tint, new Color(255, 82, 72), 0.55f);
        if (zombie.IsDead) tint = new Color(124, 120, 116) * 0.78f;
        if (_game.Art.TrySheet(id, out var sheet))
        {
            var scale = zombie.Tier == ZombieTier.Brute ? 2.8f : 2.2f;
            var source = sheet.SourceAt((float)gameTime.TotalGameTime.TotalSeconds);
            var origin = new Vector2(sheet.FrameWidth * 0.5f, sheet.FrameHeight * 0.82f);
            var rotation = zombie.IsDead ? (flip ? -MathHelper.PiOver2 : MathHelper.PiOver2) : 0f;
            batch.Draw(sheet.Texture, zombie.Position, source, tint, rotation, origin, scale, flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);
        }
        else
        {
            var texture = _game.Art.Texture("zombie");
            var size = zombie.Tier == ZombieTier.Brute ? new Point(45, 55) : new Point(34, 42);
            batch.Draw(texture, new Rectangle((int)zombie.Position.X - size.X / 2, (int)zombie.Position.Y - size.Y + 10, size.X, size.Y), tint);
        }
        if (!zombie.IsDead && zombie.Health < zombie.MaxHealth)
        {
            DrawWorldBar(batch, new Vector2(zombie.Position.X - 18, zombie.Position.Y - 48), 36, zombie.Health / zombie.MaxHealth, new Color(190, 48, 42));
        }
    }

    private void DrawPickup(SpriteBatch batch, Pickup pickup)
    {
        var key = pickup.Item switch
        {
            ItemId.Wood => "wood",
            ItemId.Scrap => "scrap",
            ItemId.Food => "food",
            ItemId.Water => "water",
            ItemId.Bandage => "medkit",
            ItemId.Pistol => "pistol",
            _ => "scrap"
        };
        var texture = _game.Art.Texture(key);
        batch.Draw(texture, new Rectangle((int)pickup.Position.X - 10, (int)pickup.Position.Y - 10, 20, 20), Color.White);
    }

    private void DrawGroundEffects(SpriteBatch batch, WorldSession session, Rectangle bounds)
    {
        foreach (var effect in session.Effects)
        {
            if (effect.Kind != WorldEffectKind.Blood) continue;
            var texture = _game.Art.Texture("blood");
            var dest = new Rectangle((int)effect.Position.X - 18, (int)effect.Position.Y - 14, 36, 28);
            if (dest.Intersects(bounds)) batch.Draw(texture, dest, Color.White * 0.76f);
        }
    }

    private void DrawForegroundEffects(SpriteBatch batch, WorldSession session)
    {
        foreach (var effect in session.Effects)
        {
            if (effect.Kind == WorldEffectKind.Blood) continue;
            var alpha = 1f - effect.NormalizedAge;
            switch (effect.Kind)
            {
                case WorldEffectKind.Slash:
                    DrawSlash(batch, effect, alpha);
                    break;
                case WorldEffectKind.DamageText:
                    _game.Font.DrawShadow(batch, effect.Text, effect.Position, effect.Color * alpha, 2);
                    break;
                case WorldEffectKind.HitSpark:
                    DrawSpark(batch, effect.Position, effect.Color * alpha);
                    break;
                case WorldEffectKind.GatherDust:
                    DrawDust(batch, effect.Position, new Color(164, 151, 126) * alpha);
                    break;
                case WorldEffectKind.DeathPuff:
                    DrawDust(batch, effect.Position, new Color(82, 90, 80) * alpha);
                    break;
            }
        }
    }

    private void DrawSlash(SpriteBatch batch, WorldEffect effect, float alpha)
    {
        var texture = _game.Art.Texture("slash");
        var angle = effect.Velocity.LengthSquared() > 0.01f ? MathF.Atan2(effect.Velocity.Y, effect.Velocity.X) : 0f;
        batch.Draw(texture, effect.Position, null, Color.White * alpha, angle, new Vector2(texture.Width * 0.5f, texture.Height * 0.5f), 1.7f, SpriteEffects.None, 0f);
    }

    private void DrawSpark(SpriteBatch batch, Vector2 position, Color color)
    {
        for (var i = 0; i < 5; i++)
        {
            var dx = (i - 2) * 4;
            var rect = new Rectangle((int)position.X + dx, (int)position.Y - Math.Abs(dx), 4, 4);
            batch.Draw(_game.Art.Pixel, rect, color);
        }
    }

    private void DrawDust(SpriteBatch batch, Vector2 position, Color color)
    {
        batch.Draw(_game.Art.Pixel, new Rectangle((int)position.X - 8, (int)position.Y - 2, 6, 4), color);
        batch.Draw(_game.Art.Pixel, new Rectangle((int)position.X + 2, (int)position.Y - 6, 8, 5), color * 0.8f);
        batch.Draw(_game.Art.Pixel, new Rectangle((int)position.X - 2, (int)position.Y + 4, 5, 3), color * 0.65f);
    }

    private void DrawWorldBar(SpriteBatch batch, Vector2 pos, int width, float value, Color fill)
    {
        var back = new Rectangle((int)pos.X, (int)pos.Y, width, 5);
        batch.Draw(_game.Art.Pixel, back, Color.Black * 0.75f);
        batch.Draw(_game.Art.Pixel, new Rectangle(back.X + 1, back.Y + 1, Math.Max(0, (int)((width - 2) * MathHelper.Clamp(value, 0f, 1f))), 3), fill);
    }

    private void DrawNightOverlay(SpriteBatch batch, WorldSession session)
    {
        var t = session.State.WorldTimeSeconds / Balance.DayLengthSeconds;
        var night = t < 0.20f ? 1f - t / 0.20f : t > 0.72f ? (t - 0.72f) / 0.28f : 0f;
        if (night <= 0.01f) return;
        var viewport = _game.GraphicsDevice.Viewport.Bounds;
        batch.End();
        batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, transformMatrix: null);
        batch.Draw(_game.Art.Pixel, viewport, new Color(0, 8, 20) * MathHelper.Clamp(night * 0.58f, 0f, 0.58f));
        batch.End();
        batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, transformMatrix: _game.Camera.GetTransform(_game.GraphicsDevice));
    }
}
