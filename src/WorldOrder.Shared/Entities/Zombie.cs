using Microsoft.Xna.Framework;
using WorldOrder.Core;

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
    private Vector2 _desired;

    public Zombie(Vector2 position, ZombieTier tier) : base(position)
    {
        Tier = tier;
        Health = tier == ZombieTier.Brute ? 95f : 45f;
    }

    public ZombieTier Tier { get; }
    public float Health { get; private set; }
    public Vector2 Facing { get; private set; } = new(0f, 1f);
    public override RectangleF Bounds => new(Position.X - 9, Position.Y - 9, 18, 18);

    public override void Update(WorldSession session, GameTime gameTime)
    {
        var dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _attackCooldown = Math.Max(0f, _attackCooldown - dt);
        _repathTimer -= dt;
        var toPlayer = session.Player.Position - Position;
        var distSq = toPlayer.LengthSquared();

        if (_repathTimer <= 0f)
        {
            _repathTimer = 0.22f;
            _desired = DetermineDesiredDirection(session, toPlayer, distSq);
        }

        if (_desired.LengthSquared() > 0.01f) Facing = MathTools.SafeNormalize(_desired);
        var speed = Tier == ZombieTier.Brute ? 62f : 78f;
        var delta = MathTools.SafeNormalize(_desired) * speed * dt;
        MoveWithCollision(session, delta);

        if (distSq < Balance.ZombieAttackRange * Balance.ZombieAttackRange && _attackCooldown <= 0f)
        {
            _attackCooldown = Tier == ZombieTier.Brute ? 1.35f : Balance.ZombieAttackSeconds;
            var damage = Tier == ZombieTier.Brute ? 17f : 9f;
            session.DamagePlayer(damage, 4f);
        }
    }

    public void Damage(float amount)
    {
        Health -= amount;
        if (Health <= 0f) Removed = true;
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
