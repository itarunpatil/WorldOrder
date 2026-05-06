using Microsoft.Xna.Framework;
using WorldOrder.Core;
using WorldOrder.Gameplay;

namespace WorldOrder.Entities;

public sealed class Pickup : Entity
{
    public Pickup(Vector2 position, ItemId item, int count) : base(position)
    {
        Item = item;
        Count = count;
    }

    public ItemId Item { get; }
    public int Count { get; }
    public float LifeSeconds { get; private set; } = 120f;
    public override RectangleF Bounds => new(Position.X - 8, Position.Y - 8, 16, 16);

    public override void Update(WorldSession session, GameTime gameTime)
    {
        var dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        LifeSeconds -= dt;
        if (LifeSeconds <= 0f) Removed = true;
        if (Vector2.DistanceSquared(Position, session.Player.Position) <= 30f * 30f)
        {
            session.State.Inventory.Add(Item, Count);
            Removed = true;
            session.Log($"PICKED {Count} {Item}".ToUpperInvariant());
        }
    }
}
