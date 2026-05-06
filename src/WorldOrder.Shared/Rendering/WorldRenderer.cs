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
                batch.Draw(_game.Art.Tile(tile), dest, Color.White);
            }
        }

        foreach (var chunk in session.Chunks.LoadedChunks)
        {
            if (!chunk.WorldRectangle.Intersects(bounds)) continue;
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
        DrawNightOverlay(batch, session);
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
            _ => _game.Art.Texture("crate")
        };
        var scale = node.Kind == ResourceKind.Tree ? 1.6f : 1.3f;
        var dest = new Rectangle((int)(node.Position.X - texture.Width * scale * 0.5f), (int)(node.Position.Y - texture.Height * scale * 0.72f), (int)(texture.Width * scale), (int)(texture.Height * scale));
        batch.Draw(texture, dest, Color.White);
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
            var dest = new Rectangle((int)(player.Position.X - sheet.FrameWidth * scale * 0.5f), (int)(player.Position.Y - sheet.FrameHeight * scale * 0.82f), (int)(sheet.FrameWidth * scale), (int)(sheet.FrameHeight * scale));
            batch.Draw(sheet.Texture, dest, source, Color.White, 0f, Vector2.Zero, flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);
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
        if (_game.Art.TrySheet(id, out var sheet))
        {
            var scale = zombie.Tier == ZombieTier.Brute ? 2.8f : 2.2f;
            var source = sheet.SourceAt((float)gameTime.TotalGameTime.TotalSeconds);
            var dest = new Rectangle((int)(zombie.Position.X - sheet.FrameWidth * scale * 0.5f), (int)(zombie.Position.Y - sheet.FrameHeight * scale * 0.82f), (int)(sheet.FrameWidth * scale), (int)(sheet.FrameHeight * scale));
            batch.Draw(sheet.Texture, dest, source, zombie.Tier == ZombieTier.Brute ? new Color(205, 205, 205) : Color.White, 0f, Vector2.Zero, flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);
        }
        else
        {
            var texture = _game.Art.Texture("zombie");
            var size = zombie.Tier == ZombieTier.Brute ? new Point(45, 55) : new Point(34, 42);
            batch.Draw(texture, new Rectangle((int)zombie.Position.X - size.X / 2, (int)zombie.Position.Y - size.Y + 10, size.X, size.Y), Color.White);
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
