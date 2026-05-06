using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WorldOrder.Assets;

public sealed class SpriteSheet
{
    public SpriteSheet(Texture2D texture, int frames, float frameSeconds)
    {
        Texture = texture;
        Frames = Math.Max(1, frames);
        FrameSeconds = Math.Max(0.01f, frameSeconds);
        FrameWidth = Math.Max(1, texture.Width / Frames);
        FrameHeight = texture.Height;
    }

    public Texture2D Texture { get; }
    public int Frames { get; }
    public int FrameWidth { get; }
    public int FrameHeight { get; }
    public float FrameSeconds { get; }

    public Rectangle SourceAt(float worldTime)
    {
        var frame = (int)MathF.Floor(worldTime / FrameSeconds) % Frames;
        return new Rectangle(frame * FrameWidth, 0, FrameWidth, FrameHeight);
    }
}
