using Microsoft.Xna.Framework;

namespace WorldOrder.World;

public enum WorldEffectKind
{
    Slash,
    HitSpark,
    DamageText,
    Blood,
    DeathPuff,
    GatherDust
}

public sealed class WorldEffect
{
    public WorldEffect(WorldEffectKind kind, Vector2 position, Vector2 velocity, float lifetime, string? text = null, Color? color = null)
    {
        Kind = kind;
        Position = position;
        Velocity = velocity;
        Lifetime = lifetime;
        Remaining = lifetime;
        Text = text ?? string.Empty;
        Color = color ?? Color.White;
    }

    public WorldEffectKind Kind { get; }
    public Vector2 Position { get; private set; }
    public Vector2 Velocity { get; }
    public float Lifetime { get; }
    public float Remaining { get; private set; }
    public string Text { get; }
    public Color Color { get; }
    public float NormalizedAge => Lifetime <= 0f ? 1f : 1f - MathHelper.Clamp(Remaining / Lifetime, 0f, 1f);
    public bool Done => Remaining <= 0f;

    public void Update(float dt)
    {
        Remaining -= dt;
        Position += Velocity * dt;
    }
}
