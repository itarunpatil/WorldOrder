using Microsoft.Xna.Framework;

namespace WorldOrder.Core;

public readonly struct RectangleF
{
    public RectangleF(float x, float y, float width, float height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    public float X { get; }
    public float Y { get; }
    public float Width { get; }
    public float Height { get; }
    public float Left => X;
    public float Right => X + Width;
    public float Top => Y;
    public float Bottom => Y + Height;
    public Vector2 Center => new(X + Width * 0.5f, Y + Height * 0.5f);

    public bool Intersects(RectangleF other)
    {
        return other.Left < Right && Left < other.Right && other.Top < Bottom && Top < other.Bottom;
    }

    public Rectangle ToRectangle()
    {
        return new Rectangle((int)MathF.Floor(X), (int)MathF.Floor(Y), (int)MathF.Ceiling(Width), (int)MathF.Ceiling(Height));
    }
}
