using Microsoft.Xna.Framework;
using WorldOrder.Core;
using WorldOrder.World;

namespace WorldOrder.Entities;

public enum ZombieTier
{
    Walker,
    Brute
}

public sealed class Zombie : Entity
{
    private float _attackCooldown;
    private float _repathTimer;
    private float _deathTimer;
    private Vector2 _desired;
    private Vector2 _knockbackVelocity;

    public Zombie(Vector2 position, ZombieTier tier) : base(position)
    {
        Tier = tier;
        MaxHealth = tier == ZombieTier.Brute ? 95f : 45f;
        Health = MaxHealth;
    }

    public ZombieTier Tier { get; }
    public float Health { get; private set; }
    public float MaxHealth { get; }
    public float HurtTimer { get; private set; }
    public bool IsDead { get; private set; }
    public Vector2 Facing { get; private set; } = new(0f, 1f);
    public override RectangleF Bounds => new(Position.X - 9, Position.Y - 9, 18, 18);

    public override void Update(WorldSession session, GameTime gameTime)
    {
        var dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        HurtTimer = Math.Max(0f, HurtTimer - dt);

        if (_knockbackVelocity.LengthSquared() > 1f)
        {
            MoveWithCollision(session, _knockbackVelocity * dt);
            _knockbackVelocity = Vector2.Lerp(_knockbackVelocity, Vector2.Zero, 1f - MathF.Pow(0.012f, dt));
        }

        if (IsDead)
        {
            _deathTimer -= dt;
            if (_deathTimer <= 0f) Removed = true;
            return;
        }

        _attackCooldown = Math.Max(0f, _attackCooldown - dt);
        _repathTimer -= dt;
        var toPlayer = session.Player.Position - Position;
        var distSq = toPlayer.LengthSquared();

        if (_repathTimer <= 0f)
        {
            _repathTimer = 0.18f;
            _desired = DetermineDesiredDirection(session, toPlayer, distSq);
        }

        if (_desired.LengthSquared() > 0.01f) Facing = MathTools.SafeNormalize(_desired);
        var speed = Tier == ZombieTier.Brute ? 58f : 80f;
        var delta = MathTools.SafeNormalize(_desired) * speed * dt;
        MoveWithCollision(session, delta);

        if (distSq < Balance.ZombieAttackRange * Balance.ZombieAttackRange && _attackCooldown <= 0f)
        {
            _attackCooldown = Tier == ZombieTier.Brute ? 1.35f : Balance.ZombieAttackSeconds;
            var damage = Tier == ZombieTier.Brute ? 17f : 9f;
            session.EmitEffect(WorldEffectKind.Slash, Position + Facing * 18f, Facing * 18f, 0.14f, null, new Color(180, 220, 160));
            session.DamagePlayer(damage, 4f);
        }
    }

    public void Damage(float amount, Vector2 knockback)
    {
        if (IsDead) return;
        Health = Math.Max(0f, Health - amount);
        HurtTimer = 0.18f;
        _knockbackVelocity += knockback;
        if (Health <= 0f)
        {
            IsDead = true;
            _deathTimer = 0.72f;
            _desired = Vector2.Zero;
        }
    }

    private Vector2 DetermineDesiredDirection(WorldSession session, Vector2 toPlayer, float distSq)
    {
        if (distSq < Balance.ZombieSightRange * Balance.ZombieSightRange)
        {
            return toPlayer;
        }
        var cellX = (int)MathF.Floor(Position.X / 180f);
        var cellY = (int)MathF.Floor(Position.Y / 180f);
        var angle = Hashing.Unit(cellX, cellY, session.State.Seed + 900) * MathF.Tau;
        return new Vector2(MathF.Cos(angle), MathF.Sin(angle));
    }

    private void MoveWithCollision(WorldSession session, Vector2 delta)
    {
        if (delta.LengthSquared() < 0.001f) return;
        var next = Position + new Vector2(delta.X, 0f);
        if (!session.Chunks.IsBlocked(next)) Position = next;
        next = Position + new Vector2(0f, delta.Y);
        if (!session.Chunks.IsBlocked(next)) Position = next;
    }
}
