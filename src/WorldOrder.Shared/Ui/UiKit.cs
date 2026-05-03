using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WorldOrder;

public readonly record struct UiResult(bool Hovered, bool Clicked);

public static class UiKit
{
    public static readonly Color Ink = new(245, 239, 219);
    public static readonly Color InkDim = new(191, 178, 148);
    public static readonly Color Panel = new(42, 35, 28, 236);
    public static readonly Color PanelLight = new(66, 54, 40, 236);
    public static readonly Color Accent = new(215, 154, 68);
    public static readonly Color Green = new(113, 205, 126);
    public static readonly Color Red = new(214, 82, 62);

    public static UiResult Button(Game1 game, SpriteBatch batch, InputState input, Rectangle rect, string label, bool enabled = true, float scale = 0.85f)
    {
        bool hover = enabled && rect.Contains(input.Pointer);
        bool clicked = enabled && hover && input.PointerReleased;
        Color fill = enabled ? (hover ? new Color(85, 66, 44, 242) : new Color(54, 43, 33, 242)) : new Color(34, 31, 28, 180);
        Color border = enabled ? (hover ? new Color(245, 193, 93) : new Color(145, 107, 59)) : new Color(90, 82, 72);
        Fill(batch, game.Pixel, rect, fill);
        Fill(batch, game.Pixel, new Rectangle(rect.X, rect.Y, rect.Width, 3), border);
        Fill(batch, game.Pixel, new Rectangle(rect.X, rect.Bottom - 3, rect.Width, 3), new Color(25, 20, 17, 230));
        Fill(batch, game.Pixel, new Rectangle(rect.X, rect.Y, 3, rect.Height), border);
        Fill(batch, game.Pixel, new Rectangle(rect.Right - 3, rect.Y, 3, rect.Height), border);
        var textColor = enabled ? Ink : InkDim * 0.55f;
        DrawCenteredFitted(game, batch, label, rect, textColor, scale, 18);
        return new UiResult(hover, clicked);
    }

    public static void PanelBox(Game1 game, SpriteBatch batch, Rectangle rect, string? title = null)
    {
        Fill(batch, game.Pixel, rect, Panel);
        Fill(batch, game.Pixel, new Rectangle(rect.X, rect.Y, rect.Width, 4), Accent);
        Fill(batch, game.Pixel, new Rectangle(rect.X, rect.Bottom - 2, rect.Width, 2), new Color(12, 10, 8, 235));
        Fill(batch, game.Pixel, new Rectangle(rect.X, rect.Y, 2, rect.Height), new Color(110, 81, 48));
        Fill(batch, game.Pixel, new Rectangle(rect.Right - 2, rect.Y, 2, rect.Height), new Color(12, 10, 8, 235));
        if (!string.IsNullOrWhiteSpace(title))
        {
            DrawFitted(game, batch, title.ToUpperInvariant(), new Vector2(rect.X + 18, rect.Y + 12), Ink, 0.78f, rect.Width - 36);
        }
    }

    public static void Fill(SpriteBatch batch, Texture2D pixel, Rectangle rect, Color color)
    {
        if (rect.Width <= 0 || rect.Height <= 0) return;
        batch.Draw(pixel, rect, color);
    }

    public static void Outline(SpriteBatch batch, Texture2D pixel, Rectangle rect, Color color, int thickness = 2)
    {
        Fill(batch, pixel, new Rectangle(rect.X, rect.Y, rect.Width, thickness), color);
        Fill(batch, pixel, new Rectangle(rect.X, rect.Bottom - thickness, rect.Width, thickness), color);
        Fill(batch, pixel, new Rectangle(rect.X, rect.Y, thickness, rect.Height), color);
        Fill(batch, pixel, new Rectangle(rect.Right - thickness, rect.Y, thickness, rect.Height), color);
    }

    public static Rectangle Centered(int width, int height, int w, int h, int yOffset = 0)
    {
        return new Rectangle(width / 2 - w / 2, height / 2 - h / 2 + yOffset, w, h);
    }

    public static Rectangle ResponsivePanel(int viewportWidth, int viewportHeight, int preferredWidth, int preferredHeight, int margin = 36)
    {
        int width = System.Math.Clamp(preferredWidth, 320, System.Math.Max(320, viewportWidth - margin * 2));
        int height = System.Math.Clamp(preferredHeight, 300, System.Math.Max(300, viewportHeight - margin * 2));
        return Centered(viewportWidth, viewportHeight, width, height);
    }

    public static void DrawFitted(Game1 game, SpriteBatch batch, string text, Vector2 position, Color color, float scale, float maxWidth)
    {
        float measured = game.Font.Measure(text, 1f).X;
        if (measured > 1f && measured * scale > maxWidth)
        {
            scale = MathHelper.Max(0.42f, maxWidth / measured);
        }
        game.Font.Draw(batch, text, position, color, scale);
    }

    public static void DrawCenteredFitted(Game1 game, SpriteBatch batch, string text, Rectangle rect, Color color, float scale, int padding = 12)
    {
        float measured = game.Font.Measure(text, 1f).X;
        float maxWidth = System.Math.Max(16, rect.Width - padding * 2);
        if (measured > 1f && measured * scale > maxWidth)
        {
            scale = MathHelper.Max(0.38f, maxWidth / measured);
        }
        game.Font.DrawCentered(batch, text, rect, color, scale);
    }

    public static void ProgressBar(Game1 game, SpriteBatch batch, Rectangle rect, float progress, string label)
    {
        Fill(batch, game.Pixel, rect, new Color(28, 24, 20, 240));
        Outline(batch, game.Pixel, rect, new Color(142, 103, 56), 2);
        var inner = new Rectangle(rect.X + 4, rect.Y + 4, (int)((rect.Width - 8) * MathEx.Clamp01(progress)), rect.Height - 8);
        Fill(batch, game.Pixel, inner, Accent);
        DrawCenteredFitted(game, batch, label, rect, Ink, 0.68f, 18);
    }
}
