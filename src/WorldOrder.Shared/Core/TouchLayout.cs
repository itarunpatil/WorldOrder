using Microsoft.Xna.Framework;

namespace WorldOrder.Core;

public static class TouchLayout
{
    public static Rectangle MovePad(Rectangle viewport)
    {
        var size = Math.Clamp(Math.Min(viewport.Width, viewport.Height) / 3, 176, 292);
        var origin = MoveOrigin(viewport);
        return new Rectangle((int)origin.X - size / 2, (int)origin.Y - size / 2, size, size);
    }

    public static Vector2 MoveOrigin(Rectangle viewport) => new(MathF.Max(112f, viewport.Width * 0.12f), viewport.Height - MathF.Max(116f, viewport.Height * 0.20f));

    public static Rectangle Attack(Rectangle viewport) => CircleButton(viewport, 1, 1);
    public static Rectangle Gather(Rectangle viewport) => CircleButton(viewport, 2, 2);
    public static Rectangle Build(Rectangle viewport) => CircleButton(viewport, 3, 2);
    public static Rectangle Eat(Rectangle viewport) => CircleButton(viewport, 2, 1);
    public static Rectangle Heal(Rectangle viewport) => CircleButton(viewport, 3, 1);
    public static Rectangle Pause(Rectangle viewport) => new(viewport.Width - 88, 18, 64, 48);

    public static bool IsGameplayControl(Rectangle viewport, Vector2 point)
    {
        return MovePad(viewport).Contains(point.ToPoint())
            || Attack(viewport).Contains(point.ToPoint())
            || Gather(viewport).Contains(point.ToPoint())
            || Build(viewport).Contains(point.ToPoint())
            || Eat(viewport).Contains(point.ToPoint())
            || Heal(viewport).Contains(point.ToPoint())
            || Pause(viewport).Contains(point.ToPoint());
    }

    private static Rectangle CircleButton(Rectangle viewport, int columnFromRight, int rowFromBottom)
    {
        var size = Math.Clamp(viewport.Height / 9, 58, 88);
        var gap = Math.Clamp(viewport.Height / 34, 12, 24);
        var x = viewport.Width - gap - columnFromRight * size - (columnFromRight - 1) * gap;
        var y = viewport.Height - gap - rowFromBottom * size - (rowFromBottom - 1) * gap;
        return new Rectangle(x, y, size, size);
    }
}
