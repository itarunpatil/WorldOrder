using Microsoft.Xna.Framework;
using WorldOrder.Core;

namespace WorldOrder.Entities;

public abstract class Entity
{
    protected Entity(Vector2 position) => Position = position;
    public Vector2 Position { get; set; }
    public Vector2 Velocity { get; set; }
    public bool Removed { get; set; }
    public abstract RectangleF Bounds { get; }
    public virtual void Update(WorldSession session, GameTime gameTime) { }
}
