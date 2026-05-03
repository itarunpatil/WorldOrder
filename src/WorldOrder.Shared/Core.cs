using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WorldOrder;

public interface IGameScreen
{
    void OnEnter() { }
    void OnExit() { }
    void Update(GameTime time, InputState input);
    void Draw(GameTime time, SpriteBatch batch);
}

public sealed class ScreenStack
{
    private readonly Game1 _game;
    public IGameScreen? Current { get; private set; }

    public ScreenStack(Game1 game) => _game = game;

    public void Change(IGameScreen screen)
    {
        Current?.OnExit();
        Current = screen;
        Current.OnEnter();
    }

    public void Update(GameTime time, InputState input) => Current?.Update(time, input);
    public void Draw(GameTime time, SpriteBatch batch) => Current?.Draw(time, batch);
}

public static class MathEx
{
    public static float Dt(GameTime time) => (float)time.ElapsedGameTime.TotalSeconds;
    public static float Clamp01(float value) => MathHelper.Clamp(value, 0f, 1f);
    public static float Approach(float value, float target, float amount)
    {
        if (value < target) return MathHelper.Min(value + amount, target);
        if (value > target) return MathHelper.Max(value - amount, target);
        return value;
    }

    public static Vector2 FromAngle(float angle) => new((float)System.Math.Cos(angle), (float)System.Math.Sin(angle));

    public static float Angle(Vector2 from, Vector2 to)
    {
        var d = to - from;
        if (d.LengthSquared() < 0.0001f) return 0f;
        return (float)System.Math.Atan2(d.Y, d.X) + MathHelper.PiOver2;
    }

    public static float RotationToVelocity(Vector2 velocity, float fallback)
    {
        if (velocity.LengthSquared() < 0.001f) return fallback;
        return (float)System.Math.Atan2(velocity.Y, velocity.X) + MathHelper.PiOver2;
    }

    public static Rectangle RectFromPoints(Vector2 a, Vector2 b)
    {
        int x = (int)System.Math.Min(a.X, b.X);
        int y = (int)System.Math.Min(a.Y, b.Y);
        int w = (int)System.Math.Abs(a.X - b.X);
        int h = (int)System.Math.Abs(a.Y - b.Y);
        return new Rectangle(x, y, w, h);
    }
}
