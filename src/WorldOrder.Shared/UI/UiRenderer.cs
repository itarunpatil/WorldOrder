using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WorldOrder.Assets;

namespace WorldOrder.UI;

public sealed class UiRenderer
{
    private readonly ArtLibrary _art;
    private readonly PixelFont _font;

    public UiRenderer(ArtLibrary art, PixelFont font)
    {
        _art = art;
        _font = font;
    }

    public void Panel(SpriteBatch batch, Rectangle rect, Color border, Color fill)
    {
        batch.Draw(_art.Pixel, rect, fill);
        batch.Draw(_art.Pixel, new Rectangle(rect.X, rect.Y, rect.Width, 2), border);
        batch.Draw(_art.Pixel, new Rectangle(rect.X, rect.Bottom - 2, rect.Width, 2), border);
        batch.Draw(_art.Pixel, new Rectangle(rect.X, rect.Y, 2, rect.Height), border);
        batch.Draw(_art.Pixel, new Rectangle(rect.Right - 2, rect.Y, 2, rect.Height), border);
    }

    public void Bar(SpriteBatch batch, Rectangle rect, float value, Color fill, Color back)
    {
        var border = Color.Black * 0.85f;
        batch.Draw(_art.Pixel, rect, back);
        var inner = new Rectangle(rect.X + 2, rect.Y + 2, Math.Max(0, (int)((rect.Width - 4) * MathHelper.Clamp(value, 0f, 1f))), rect.Height - 4);
        batch.Draw(_art.Pixel, inner, fill);
        Panel(batch, rect, border, Color.Transparent);
    }

    public void Button(SpriteBatch batch, Rectangle rect, string label, bool selected)
    {
        var border = selected ? new Color(235, 198, 89) : new Color(89, 92, 88);
        var fill = selected ? new Color(66, 62, 43, 230) : new Color(25, 28, 27, 220);
        Panel(batch, rect, border, fill);
        var size = _font.Measure(label, 3);
        var pos = new Vector2(rect.Center.X - size.X * 0.5f, rect.Center.Y - size.Y * 0.5f);
        _font.DrawShadow(batch, label, pos, selected ? new Color(248, 238, 170) : new Color(205, 213, 204), 3);
    }

    public void Label(SpriteBatch batch, string text, Vector2 pos, Color color, int scale = 2) => _font.DrawShadow(batch, text, pos, color, scale);
}
