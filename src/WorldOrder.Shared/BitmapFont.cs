using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WorldOrder;

public sealed class BitmapFont
{
    private readonly Texture2D _texture;
    private readonly int _cellW;
    private readonly int _cellH;
    private const int Columns = 16;

    public BitmapFont(Texture2D texture, int cellW, int cellH)
    {
        _texture = texture;
        _cellW = cellW;
        _cellH = cellH;
    }

    public Vector2 Measure(string text, float scale = 1f)
    {
        int line = 0;
        int max = 0;
        int lines = 1;
        foreach (char ch in text)
        {
            if (ch == '\n')
            {
                max = System.Math.Max(max, line);
                line = 0;
                lines++;
            }
            else line++;
        }
        max = System.Math.Max(max, line);
        return new Vector2(max * _cellW * 0.7f * scale, lines * _cellH * scale);
    }

    public void Draw(SpriteBatch batch, string text, Vector2 position, Color color, float scale = 1f, float alpha = 1f)
    {
        var origin = position;
        Vector2 p = position;
        foreach (char raw in text)
        {
            if (raw == '\n')
            {
                p.X = origin.X;
                p.Y += _cellH * scale;
                continue;
            }

            char ch = raw < 32 || raw > 127 ? '?' : raw;
            int idx = ch - 32;
            var src = new Rectangle((idx % Columns) * _cellW, (idx / Columns) * _cellH, _cellW, _cellH);
            batch.Draw(_texture, p, src, color * alpha, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
            p.X += _cellW * 0.7f * scale;
        }
    }

    public void DrawCentered(SpriteBatch batch, string text, Rectangle rect, Color color, float scale = 1f)
    {
        var size = Measure(text, scale);
        Draw(batch, text, new Vector2(rect.Center.X - size.X / 2f, rect.Center.Y - size.Y / 2f), color, scale);
    }
}
