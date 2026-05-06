using Microsoft.Xna.Framework;

namespace WorldOrder.Core;

public static class MathTools
{
    public static int FloorDiv(int value, int divisor)
    {
        var result = value / divisor;
        var rem = value % divisor;
        if (rem != 0 && ((rem < 0) != (divisor < 0))) result--;
        return result;
    }

    public static int PositiveModulo(int value, int mod)
    {
        var result = value % mod;
        return result < 0 ? result + mod : result;
    }

    public static Vector2 SafeNormalize(Vector2 value)
    {
        if (value.LengthSquared() <= 0.0001f) return Vector2.Zero;
        value.Normalize();
        return value;
    }

    public static Rectangle Expand(Rectangle rect, int pixels)
    {
        return new Rectangle(rect.X - pixels, rect.Y - pixels, rect.Width + pixels * 2, rect.Height + pixels * 2);
    }

    public static int ClampInt(int value, int min, int max)
    {
        if (value < min) return min;
        return value > max ? max : value;
    }

    public static float Approach(float current, float target, float delta)
    {
        if (current < target) return Math.Min(current + delta, target);
        return Math.Max(current - delta, target);
    }
}
