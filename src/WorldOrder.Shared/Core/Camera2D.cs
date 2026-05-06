using Microsoft.Xna.Framework;

namespace WorldOrder.Core;

public sealed class Camera2D
{
    public Vector2 Position { get; private set; }
    public float Zoom { get; private set; } = 2f;

    public Matrix GetTransform(GraphicsDevice graphicsDevice)
    {
        var viewport = graphicsDevice.Viewport;
        return Matrix.CreateTranslation(new Vector3(-Position, 0f)) *
               Matrix.CreateScale(Zoom, Zoom, 1f) *
               Matrix.CreateTranslation(viewport.Width * 0.5f, viewport.Height * 0.5f, 0f);
    }

    public Rectangle GetWorldBounds(GraphicsDevice graphicsDevice, int marginPixels = 128)
    {
        var viewport = graphicsDevice.Viewport;
        var halfWidth = viewport.Width / Zoom * 0.5f;
        var halfHeight = viewport.Height / Zoom * 0.5f;
        return new Rectangle(
            (int)MathF.Floor(Position.X - halfWidth) - marginPixels,
            (int)MathF.Floor(Position.Y - halfHeight) - marginPixels,
            (int)MathF.Ceiling(halfWidth * 2f) + marginPixels * 2,
            (int)MathF.Ceiling(halfHeight * 2f) + marginPixels * 2);
    }

    public Vector2 ScreenToWorld(GraphicsDevice graphicsDevice, Vector2 screen)
    {
        var viewport = graphicsDevice.Viewport;
        return (screen - new Vector2(viewport.Width * 0.5f, viewport.Height * 0.5f)) / Zoom + Position;
    }

    public void Follow(Vector2 target, float elapsed)
    {
        var stiffness = 1f - MathF.Pow(0.001f, elapsed);
        Position = Vector2.Lerp(Position, target, stiffness);
    }

    public void Snap(Vector2 target) => Position = target;

    public void AdjustZoom(float wheelDelta)
    {
        Zoom = MathHelper.Clamp(Zoom + wheelDelta * 0.001f, 1.2f, 3.5f);
    }
}
