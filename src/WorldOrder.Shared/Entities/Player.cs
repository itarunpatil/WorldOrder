using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using WorldOrder.Core;
using WorldOrder.Gameplay;
using WorldOrder.World;

namespace WorldOrder.Entities;

public sealed class Player : Entity
{
    private float _attackTimer;
    private float _useTimer;

    public Player(Vector2 position) : base(position) { }

    public Vector2 Facing { get; private set; } = new(0f, 1f);
    public bool IsMoving { get; private set; }
    public bool IsAttacking => _attackTimer > 0f;
    public float AttackCharge => MathHelper.Clamp(_attackTimer / Balance.PlayerAttackSeconds, 0f, 1f);
    public override RectangleF Bounds => new(Position.X - 9, Position.Y - 9, 18, 18);

    public override void Update(WorldSession session, GameTime gameTime)
    {
        var dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        var input = session.Game.Input;
        var viewport = session.Game.GraphicsDevice.Viewport.Bounds;
        _attackTimer = Math.Max(0f, _attackTimer - dt);
        _useTimer = Math.Max(0f, _useTimer - dt);
        if (session.CraftingOpen) return;

        var movement = input.Movement;
        IsMoving = movement.LengthSquared() > 0.01f;
        if (IsMoving) Facing = movement;

        var sprint = input.Down(Keys.LeftShift) && session.State.Vitals.Stamina > 5f && IsMoving;
        var speed = sprint ? Balance.PlayerSprintSpeed : Balance.PlayerWalkSpeed;
        if (sprint) session.State.Vitals.Stamina = Math.Max(0f, session.State.Vitals.Stamina - dt * 24f);
        else session.State.Vitals.Stamina = Math.Min(100f, session.State.Vitals.Stamina + dt * 15f);

        MoveWithCollision(session, movement * speed * dt);

        var gatherPressed = input.Pressed(Keys.E) || input.RightClick || input.Tapped(TouchLayout.Gather(viewport));
        if (gatherPressed && _useTimer <= 0f && !session.BuildMode)
        {
            _useTimer = 0.18f;
            if (input.RightClick) FacePointer(session);
            session.InteractNearestResource();
        }

        var attackPressed = input.Pressed(Keys.Space) || input.Pressed(Keys.F) || (input.LeftClick && !session.BuildMode) || input.Tapped(TouchLayout.Attack(viewport));
        if (attackPressed && _attackTimer <= 0f && !session.BuildMode)
        {
            if (input.LeftClick) FacePointer(session);
            _attackTimer = Balance.PlayerAttackSeconds;
            session.PlayerAttack();
        }

        HandleNumberKeys(session, input);
        if (input.Pressed(Keys.B) || input.Tapped(TouchLayout.Build(viewport)))
        {
            session.BuildMode = !session.BuildMode;
            session.CraftingOpen = false;
        }
        if (input.Pressed(Keys.R)) session.SaveNow();
        if (input.Pressed(Keys.H) || input.Tapped(TouchLayout.Heal(viewport))) ConsumeHealing(session);
        if (input.Pressed(Keys.Q) || input.Tapped(TouchLayout.Eat(viewport))) EatOrDrink(session);
    }

    private static void HandleNumberKeys(WorldSession session, InputState input)
    {
        var keys = new[] { Keys.D1, Keys.D2, Keys.D3, Keys.D4, Keys.D5, Keys.D6, Keys.D7, Keys.D8, Keys.D9 };
        for (var i = 0; i < keys.Length; i++)
        {
            if (!input.Pressed(keys[i])) continue;
            if (session.BuildMode && i < GameDefinitions.Buildables.Length) session.SelectedBuildableIndex = i;
            else if (i < Inventory.HotbarCapacity) session.SelectedHotbarIndex = i;
        }
    }


    private void FacePointer(WorldSession session)
    {
        var pointer = session.Game.Input.WorldPointer(session.Game.Camera, session.Game.GraphicsDevice);
        var aim = pointer - Position;
        if (aim.LengthSquared() > 16f) Facing = MathTools.SafeNormalize(aim);
    }

    private void MoveWithCollision(WorldSession session, Vector2 delta)
    {
        if (delta.LengthSquared() < 0.001f) return;
        var next = Position + new Vector2(delta.X, 0f);
        if (!BlockedAt(session, next)) Position = next;
        next = Position + new Vector2(0f, delta.Y);
        if (!BlockedAt(session, next)) Position = next;
    }

    private bool BlockedAt(WorldSession session, Vector2 position)
    {
        var points = new[]
        {
            position + new Vector2(-8, -8), position + new Vector2(8, -8),
            position + new Vector2(-8, 8), position + new Vector2(8, 8)
        };
        return points.Any(session.Chunks.IsBlocked);
    }

    private void EatOrDrink(WorldSession session)
    {
        var vitals = session.State.Vitals;
        if (vitals.Hunger < vitals.Thirst && session.State.Inventory.Remove(ItemId.Food, 1))
        {
            vitals.Hunger = Math.Min(100f, vitals.Hunger + 32f);
            session.Log("ATE FOOD");
        }
        else if (session.State.Inventory.Remove(ItemId.Water, 1))
        {
            vitals.Thirst = Math.Min(100f, vitals.Thirst + 38f);
            session.Log("DRANK WATER");
        }
        else
        {
            session.Log("NO FOOD OR WATER");
        }
    }

    private void ConsumeHealing(WorldSession session)
    {
        if (session.State.Inventory.Remove(ItemId.Bandage, 1))
        {
            session.State.Vitals.Health = Math.Min(100f, session.State.Vitals.Health + 28f);
            session.State.Vitals.Infection = Math.Max(0f, session.State.Vitals.Infection - 12f);
            session.Log("USED BANDAGE");
        }
        else
        {
            session.Log("NO BANDAGE");
        }
    }
}
