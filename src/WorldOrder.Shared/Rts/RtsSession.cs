using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WorldOrder;

public enum FactionKind
{
    Player,
    Enemy,
    Ally,
    Neutral
}

public enum UnitKind
{
    CommandCenter,
    LightTank,
    HeavyTank,
    Harvester,
    ScoutBoat
}

public sealed class Unit
{
    private static int _nextId;
    public int Id { get; } = ++_nextId;
    public UnitKind Kind { get; init; }
    public FactionKind Faction { get; init; }
    public int Team { get; init; }
    public Vector2 Position;
    public Vector2 Velocity;
    public Vector2? MoveTarget;
    public Queue<Vector2> Path { get; } = new();
    public Unit? AttackTarget;
    public ResourceNode? HarvestTarget;
    public int Cargo;
    public int CargoCapacity { get; init; } = 80;
    public float HarvestTimer;
    public float Rotation;
    public float TurretRotation;
    public float Radius { get; init; } = 34f;
    public float Speed { get; init; } = 150f;
    public float Range { get; init; } = 390f;
    public float ReloadMax { get; init; } = 1.1f;
    public float Reload;
    public int Damage { get; init; } = 22;
    public float Health;
    public float MaxHealth { get; init; } = 120f;
    public bool Selected;
    public bool Naval => Kind == UnitKind.ScoutBoat;
    public bool Structure => Kind == UnitKind.CommandCenter;
    public bool Worker => Kind == UnitKind.Harvester;
    public string HullKey { get; init; } = "hull_player_01";
    public string GunKey { get; init; } = "gun_player_01";
    public string BoatKey { get; init; } = "boat_water_1_1_1";
    public float SpawnCooldown;

    public bool Alive => Health > 0f;
    public bool IsHostileTo(Unit other) => Faction != other.Faction && Faction != FactionKind.Neutral && other.Faction != FactionKind.Neutral;

    public RectangleF Bounds => new(Position.X - Radius, Position.Y - Radius, Radius * 2, Radius * 2);
}

public readonly record struct RectangleF(float X, float Y, float Width, float Height)
{
    public bool Contains(Vector2 p) => p.X >= X && p.Y >= Y && p.X <= X + Width && p.Y <= Y + Height;
    public bool Intersects(Rectangle r) => X < r.Right && X + Width > r.Left && Y < r.Bottom && Y + Height > r.Top;
}

public sealed class Projectile
{
    public Vector2 Position;
    public Vector2 Velocity;
    public Unit? Target;
    public Unit Owner = null!;
    public int Damage;
    public float Life = 2f;
}

public sealed class Particle
{
    public Vector2 Position;
    public string Prefix = "explosion_";
    public float Age;
    public float Duration = 0.58f;
    public float Scale = 0.75f;
}

public sealed class RtsSession
{
    private readonly Game1 _game;
    private readonly Random _random;
    private float _aiTicker;
    private float _resourceTicker;
    private int _wave;

    public WorldSettings Settings { get; }
    public GameMap Map { get; }
    public List<Unit> Units { get; } = new();
    public List<Projectile> Projectiles { get; } = new();
    public List<Particle> Particles { get; } = new();
    public int Supplies { get; private set; } = 420;
    public bool[,] VisibleTiles { get; }
    public bool[,] ExploredTiles { get; }
    public string Objective { get; private set; } = "Destroy enemy command centers";
    public bool Victory { get; private set; }
    public bool Defeat { get; private set; }

    private RtsSession(Game1 game, WorldSettings settings, GameMap map)
    {
        _game = game;
        Settings = settings;
        Map = map;
        _random = new Random(settings.Seed ^ 0x51C0FFEE);
        VisibleTiles = new bool[map.Width, map.Height];
        ExploredTiles = new bool[map.Width, map.Height];
    }

    public static RtsSession Create(Game1 game, WorldSettings settings)
    {
        var map = GameMap.Generate(settings);
        var s = new RtsSession(game, settings, map);
        s.Bootstrap();
        return s;
    }

    private void Bootstrap()
    {
        AddCommandCenter(FactionKind.Player, 0, Map.PlayerSpawn);
        for (int i = 0; i < 5; i++) AddTank(FactionKind.Player, 0, Map.PlayerSpawn + new Vector2(150 + i * 60, -120 + i * 42), i % 2 == 0 ? UnitKind.LightTank : UnitKind.HeavyTank);
        AddTank(FactionKind.Player, 0, Map.PlayerSpawn + new Vector2(110, 170), UnitKind.Harvester);
        AddBoat(FactionKind.Player, 0, FindNearestWater(Map.PlayerSpawn + new Vector2(1500, 0)));

        for (int i = 0; i < Settings.EnemyFactions; i++)
        {
            var spawn = Map.EnemySpawns[i % Map.EnemySpawns.Count];
            AddCommandCenter(FactionKind.Enemy, 10 + i, spawn);
            int guards = Settings.Difficulty == Difficulty.Warlord ? 6 : Settings.Difficulty == Difficulty.Veteran ? 5 : 4;
            for (int j = 0; j < guards; j++)
            {
                AddTank(FactionKind.Enemy, 10 + i, spawn + Scatter(260), j % 3 == 0 ? UnitKind.HeavyTank : UnitKind.LightTank);
            }
        }

        for (int i = 0; i < Settings.AllyFactions; i++)
        {
            var spawn = Map.AllySpawns[i % Map.AllySpawns.Count];
            AddCommandCenter(FactionKind.Ally, 50 + i, spawn);
            for (int j = 0; j < 3; j++) AddTank(FactionKind.Ally, 50 + i, spawn + Scatter(220), UnitKind.LightTank);
        }
        UpdateVisibility();
    }

    public Unit AddCommandCenter(FactionKind faction, int team, Vector2 position)
    {
        var color = Prefix(faction);
        var unit = new Unit
        {
            Kind = UnitKind.CommandCenter,
            Faction = faction,
            Team = team,
            Position = position,
            Radius = 82f,
            Speed = 0f,
            Range = 520f,
            ReloadMax = 1.9f,
            Damage = 36,
            MaxHealth = 680f,
            Health = 680f,
            HullKey = $"hull_{color}_05",
            GunKey = $"gun_{color}_06"
        };
        Units.Add(unit);
        return unit;
    }

    public Unit AddTank(FactionKind faction, int team, Vector2 position, UnitKind kind)
    {
        var color = Prefix(faction);
        string hull = kind == UnitKind.HeavyTank ? "02" : "01";
        string gun = kind == UnitKind.HeavyTank ? "03" : "01";
        var unit = new Unit
        {
            Kind = kind,
            Faction = faction,
            Team = team,
            Position = ClampToPassable(position, false),
            Radius = kind == UnitKind.HeavyTank ? 42f : kind == UnitKind.Harvester ? 38f : 35f,
            Speed = kind == UnitKind.HeavyTank ? 124f : kind == UnitKind.Harvester ? 142f : 168f,
            Range = kind == UnitKind.HeavyTank ? 480f : kind == UnitKind.Harvester ? 260f : 400f,
            ReloadMax = kind == UnitKind.HeavyTank ? 1.38f : kind == UnitKind.Harvester ? 1.6f : 0.96f,
            Damage = kind == UnitKind.HeavyTank ? 42 : kind == UnitKind.Harvester ? 11 : 25,
            MaxHealth = kind == UnitKind.HeavyTank ? 190f : kind == UnitKind.Harvester ? 145f : 120f,
            Health = kind == UnitKind.HeavyTank ? 190f : kind == UnitKind.Harvester ? 145f : 120f,
            CargoCapacity = kind == UnitKind.Harvester ? 90 : 0,
            HullKey = $"hull_{color}_{hull}",
            GunKey = $"gun_{color}_{gun}"
        };
        Units.Add(unit);
        return unit;
    }

    public Unit AddBoat(FactionKind faction, int team, Vector2 position)
    {
        int color = faction == FactionKind.Enemy ? 2 : faction == FactionKind.Ally ? 3 : 1;
        var unit = new Unit
        {
            Kind = UnitKind.ScoutBoat,
            Faction = faction,
            Team = team,
            Position = ClampToPassable(position, true),
            Radius = 36f,
            Speed = 190f,
            Range = 360f,
            ReloadMax = 0.82f,
            Damage = 18,
            MaxHealth = 95f,
            Health = 95f,
            BoatKey = $"boat_water_1_{color}_1",
            HullKey = $"boat_{color}_1",
            GunKey = $"gun_{Prefix(faction)}_01"
        };
        Units.Add(unit);
        return unit;
    }

    private string Prefix(FactionKind faction) => faction switch
    {
        FactionKind.Enemy => "enemy",
        FactionKind.Ally => "ally",
        FactionKind.Neutral => "neutral",
        _ => "player"
    };

    private Vector2 Scatter(float radius)
    {
        float a = (float)(_random.NextDouble() * MathHelper.TwoPi);
        float r = (float)(_random.NextDouble() * radius);
        return new Vector2(MathF.Cos(a), MathF.Sin(a)) * r;
    }

    private Vector2 ClampToPassable(Vector2 pos, bool naval)
    {
        if (Map.IsPassable(pos, naval)) return pos;
        for (int ring = 1; ring < 18; ring++)
        {
            for (int i = 0; i < 16; i++)
            {
                float a = i / 16f * MathHelper.TwoPi;
                var p = pos + new Vector2(MathF.Cos(a), MathF.Sin(a)) * ring * 64;
                if (Map.IsPassable(p, naval)) return p;
            }
        }
        return pos;
    }

    private Vector2 FindNearestWater(Vector2 around)
    {
        for (int r = 0; r < 38; r++)
        {
            for (int i = 0; i < 24; i++)
            {
                float a = i / 24f * MathHelper.TwoPi;
                var p = around + new Vector2(MathF.Cos(a), MathF.Sin(a)) * r * 96;
                if (Map.TileAtWorld(p) == TileKind.Water) return p;
            }
        }
        return around;
    }

    public void Update(GameTime time)
    {
        float dt = MathEx.Dt(time);
        if (Victory || Defeat) return;
        _resourceTicker += dt;
        if (_resourceTicker >= 1f)
        {
            _resourceTicker = 0f;
            int bases = Units.Count(u => u.Alive && u.Faction == FactionKind.Player && u.Kind == UnitKind.CommandCenter);
            Supplies += 8 + bases * 12;
        }

        _aiTicker += dt;
        if (_aiTicker > 0.8f)
        {
            _aiTicker = 0f;
            RunAi();
        }

        foreach (var u in Units)
        {
            if (!u.Alive) continue;
            UpdateUnit(u, dt);
        }

        UpdateProjectiles(dt);
        for (int i = Particles.Count - 1; i >= 0; i--)
        {
            Particles[i].Age += dt;
            if (Particles[i].Age > Particles[i].Duration) Particles.RemoveAt(i);
        }
        Units.RemoveAll(u => !u.Alive);
        UpdateVisibility();
        Victory = Units.All(u => u.Faction != FactionKind.Enemy || u.Kind != UnitKind.CommandCenter);
        Defeat = Units.All(u => u.Faction != FactionKind.Player || u.Kind != UnitKind.CommandCenter);
    }

    private void UpdateUnit(Unit u, float dt)
    {
        u.Reload = MathEx.Approach(u.Reload, 0f, dt);
        u.SpawnCooldown = MathEx.Approach(u.SpawnCooldown, 0f, dt);
        if (u.AttackTarget is { Alive: false }) u.AttackTarget = null;
        if (u.Worker && UpdateHarvester(u, dt)) return;
        if (!u.Worker && u.AttackTarget == null) u.AttackTarget = FindNearestEnemy(u, u.Range * 0.92f);
        if (u.AttackTarget != null)
        {
            u.TurretRotation = MathEx.Angle(u.Position, u.AttackTarget.Position);
            float d = Vector2.Distance(u.Position, u.AttackTarget.Position);
            if (d <= u.Range && u.Reload <= 0f)
            {
                Fire(u, u.AttackTarget);
                u.Reload = u.ReloadMax;
            }
            else if (!u.Structure && d > u.Range * 0.78f)
            {
                if (!u.MoveTarget.HasValue || Vector2.DistanceSquared(u.MoveTarget.Value, u.AttackTarget.Position) > 160f * 160f)
                {
                    SetMoveOrder(u, u.AttackTarget.Position, keepAttackTarget: true);
                }
            }
        }

        if (u.Structure) return;
        if (u.MoveTarget.HasValue)
        {
            var target = u.MoveTarget.Value;
            var delta = target - u.Position;
            if (delta.LengthSquared() < 26f * 26f)
            {
                if (u.Path.Count > 0)
                {
                    u.MoveTarget = u.Path.Dequeue();
                }
                else
                {
                    u.MoveTarget = null;
                    u.Velocity = Vector2.Zero;
                }
            }
            else
            {
                delta.Normalize();
                var desired = delta * u.Speed;
                var steer = desired;
                foreach (var other in Units)
                {
                    if (other == u || !other.Alive) continue;
                    var away = u.Position - other.Position;
                    float distSq = away.LengthSquared();
                    float min = u.Radius + other.Radius + 12f;
                    if (distSq > 1f && distSq < min * min)
                    {
                        away.Normalize();
                        steer += away * 90f;
                    }
                }
                if (steer.LengthSquared() > u.Speed * u.Speed) steer = Vector2.Normalize(steer) * u.Speed;
                var next = u.Position + steer * dt;
                if (!Map.IsPassable(next, u.Naval))
                {
                    var sideA = new Vector2(-delta.Y, delta.X);
                    var sideB = new Vector2(delta.Y, -delta.X);
                    var pa = u.Position + sideA * u.Speed * dt;
                    var pb = u.Position + sideB * u.Speed * dt;
                    if (Map.IsPassable(pa, u.Naval)) next = pa;
                    else if (Map.IsPassable(pb, u.Naval)) next = pb;
                    else next = u.Position;
                }
                u.Velocity = (next - u.Position) / Math.Max(dt, 0.0001f);
                u.Position = next;
                u.Rotation = MathHelper.Lerp(u.Rotation, MathEx.RotationToVelocity(u.Velocity, u.Rotation), 0.16f);
                if (u.AttackTarget == null) u.TurretRotation = u.Rotation;
            }
        }
        else
        {
            u.Velocity *= 0.82f;
        }
    }

    private void SetMoveOrder(Unit unit, Vector2 target, bool keepAttackTarget)
    {
        if (unit.Structure) return;
        if (!keepAttackTarget) unit.AttackTarget = null;
        var finalTarget = ClampToPassable(target, unit.Naval);
        var path = Map.FindPath(unit.Position, finalTarget, unit.Naval);
        unit.Path.Clear();
        foreach (var waypoint in path)
        {
            if (Vector2.DistanceSquared(waypoint, unit.Position) > 18f * 18f)
            {
                unit.Path.Enqueue(waypoint);
            }
        }
        unit.MoveTarget = unit.Path.Count > 0 ? unit.Path.Dequeue() : finalTarget;
    }

    private bool UpdateHarvester(Unit unit, float dt)
    {
        if (unit.AttackTarget != null) return false;

        var baseUnit = Units
            .Where(u => u.Alive && u.Faction == unit.Faction && u.Kind == UnitKind.CommandCenter)
            .OrderBy(u => Vector2.DistanceSquared(u.Position, unit.Position))
            .FirstOrDefault();

        if (unit.Cargo >= unit.CargoCapacity)
        {
            if (baseUnit == null) return true;
            float distanceToBase = Vector2.Distance(unit.Position, baseUnit.Position);
            if (distanceToBase <= baseUnit.Radius + unit.Radius + 42f)
            {
                if (unit.Faction == FactionKind.Player) Supplies += unit.Cargo;
                unit.Cargo = 0;
                unit.MoveTarget = null;
                unit.Path.Clear();
            }
            else if (!unit.MoveTarget.HasValue || Vector2.DistanceSquared(unit.MoveTarget.Value, baseUnit.Position) > 150f * 150f)
            {
                SetMoveOrder(unit, baseUnit.Position + Scatter(90f), keepAttackTarget: false);
                return false;
            }
            return true;
        }

        if (unit.HarvestTarget == null || unit.HarvestTarget.Depleted)
        {
            unit.HarvestTarget = Map.ResourceNodes
                .Where(n => !n.Depleted)
                .OrderBy(n => Vector2.DistanceSquared(n.Position, unit.Position))
                .FirstOrDefault();
        }

        if (unit.HarvestTarget == null) return false;

        float distanceToNode = Vector2.Distance(unit.Position, unit.HarvestTarget.Position);
        if (distanceToNode <= unit.HarvestTarget.Radius + unit.Radius + 8f)
        {
            unit.MoveTarget = null;
            unit.Path.Clear();
            unit.Velocity *= 0.72f;
            unit.HarvestTimer += dt;
            if (unit.HarvestTimer >= 0.22f)
            {
                unit.HarvestTimer = 0f;
                int taken = Math.Min(4, unit.HarvestTarget.Amount);
                unit.HarvestTarget.Amount -= taken;
                unit.Cargo += taken;
            }
        }
        else if (!unit.MoveTarget.HasValue || Vector2.DistanceSquared(unit.MoveTarget.Value, unit.HarvestTarget.Position) > 150f * 150f)
        {
            SetMoveOrder(unit, unit.HarvestTarget.Position, keepAttackTarget: false);
            return false;
        }

        return true;
    }

    public bool IsVisibleWorld(Vector2 world)
    {
        var tile = Map.WorldToTile(world);
        return Map.IsInsideTile(tile.X, tile.Y) && VisibleTiles[tile.X, tile.Y];
    }

    public bool IsExploredWorld(Vector2 world)
    {
        var tile = Map.WorldToTile(world);
        return Map.IsInsideTile(tile.X, tile.Y) && ExploredTiles[tile.X, tile.Y];
    }

    private void UpdateVisibility()
    {
        for (int y = 0; y < Map.Height; y++)
        for (int x = 0; x < Map.Width; x++)
        {
            VisibleTiles[x, y] = false;
        }

        foreach (var unit in Units)
        {
            if (!unit.Alive || (unit.Faction != FactionKind.Player && unit.Faction != FactionKind.Ally)) continue;
            int radius = unit.Structure ? 8 : unit.Worker ? 6 : 7;
            var center = Map.WorldToTile(unit.Position);
            for (int y = center.Y - radius; y <= center.Y + radius; y++)
            for (int x = center.X - radius; x <= center.X + radius; x++)
            {
                if (!Map.IsInsideTile(x, y)) continue;
                float dx = x - center.X;
                float dy = y - center.Y;
                if (dx * dx + dy * dy > radius * radius) continue;
                VisibleTiles[x, y] = true;
                ExploredTiles[x, y] = true;
            }
        }
    }

    private Unit? FindNearestEnemy(Unit source, float range)
    {
        Unit? nearest = null;
        float best = range * range;
        foreach (var u in Units)
        {
            if (!u.Alive || !source.IsHostileTo(u)) continue;
            float d = Vector2.DistanceSquared(source.Position, u.Position);
            if (d < best)
            {
                best = d;
                nearest = u;
            }
        }
        return nearest;
    }

    private void Fire(Unit owner, Unit target)
    {
        var dir = target.Position - owner.Position;
        if (dir.LengthSquared() < 1f) return;
        dir.Normalize();
        var start = owner.Position + dir * (owner.Radius + 18f);
        Projectiles.Add(new Projectile
        {
            Owner = owner,
            Target = target,
            Position = start,
            Velocity = dir * 720f,
            Damage = owner.Damage,
            Life = 1.4f
        });
        Particles.Add(new Particle { Position = start, Prefix = "flash_", Duration = 0.15f, Scale = 0.45f });
    }

    private void UpdateProjectiles(float dt)
    {
        for (int i = Projectiles.Count - 1; i >= 0; i--)
        {
            var p = Projectiles[i];
            if (p.Target is { Alive: true })
            {
                var dir = p.Target.Position - p.Position;
                if (dir.LengthSquared() > 0.1f)
                {
                    dir.Normalize();
                    p.Velocity = Vector2.Lerp(p.Velocity, dir * 760f, 0.12f);
                }
            }
            p.Position += p.Velocity * dt;
            p.Life -= dt;
            bool remove = p.Life <= 0f;
            if (p.Target is { Alive: true } target && Vector2.DistanceSquared(p.Position, target.Position) < (target.Radius + 18f) * (target.Radius + 18f))
            {
                target.Health -= p.Damage;
                Particles.Add(new Particle { Position = target.Position + Scatter(target.Radius), Duration = 0.58f, Scale = target.Structure ? 1.15f : 0.72f });
                remove = true;
            }
            if (remove) Projectiles.RemoveAt(i);
        }
    }

    private void RunAi()
    {
        _wave++;
        float aggression = Settings.Difficulty == Difficulty.Warlord ? 1.35f : Settings.Difficulty == Difficulty.Veteran ? 1.08f : 0.82f;
        var playerTargets = Units.Where(u => u.Alive && (u.Faction == FactionKind.Player || u.Faction == FactionKind.Ally)).ToList();
        if (playerTargets.Count == 0) return;

        foreach (var unit in Units.Where(u => u.Alive && u.Faction == FactionKind.Enemy).ToList())
        {
            if (unit.Structure)
            {
                unit.SpawnCooldown -= 0.8f;
                if (unit.SpawnCooldown <= 0f)
                {
                    unit.SpawnCooldown = 9.5f / aggression;
                    AddTank(FactionKind.Enemy, unit.Team, unit.Position + Scatter(260), _random.NextDouble() < 0.25 * aggression ? UnitKind.HeavyTank : UnitKind.LightTank);
                }
                continue;
            }
            var target = playerTargets.OrderBy(t => Vector2.DistanceSquared(t.Position, unit.Position)).FirstOrDefault();
            if (target != null && (unit.MoveTarget == null || _wave % 3 == 0))
            {
                unit.AttackTarget = target;
                SetMoveOrder(unit, target.Position + Scatter(180), keepAttackTarget: true);
            }
        }

        foreach (var ally in Units.Where(u => u.Alive && u.Faction == FactionKind.Ally && !u.Structure).ToList())
        {
            var enemy = Units.Where(u => u.Alive && u.Faction == FactionKind.Enemy).OrderBy(u => Vector2.DistanceSquared(u.Position, ally.Position)).FirstOrDefault();
            if (enemy != null)
            {
                ally.AttackTarget = enemy;
                SetMoveOrder(ally, enemy.Position + Scatter(120), keepAttackTarget: true);
            }
        }
    }

    public void CommandMove(IEnumerable<Unit> selected, Vector2 target)
    {
        int i = 0;
        foreach (var u in selected.Where(u => u.Alive && u.Faction == FactionKind.Player && !u.Structure))
        {
            SetMoveOrder(u, target + FormationOffset(i++), keepAttackTarget: false);
        }
    }

    public void CommandAttack(IEnumerable<Unit> selected, Unit target)
    {
        int i = 0;
        foreach (var u in selected.Where(u => u.Alive && u.Faction == FactionKind.Player))
        {
            u.AttackTarget = target;
            if (!u.Structure) SetMoveOrder(u, target.Position + FormationOffset(i++), keepAttackTarget: true);
        }
    }

    private static Vector2 FormationOffset(int index)
    {
        int col = index % 4;
        int row = index / 4;
        return new Vector2((col - 1.5f) * 74f, (row - 1.5f) * 74f);
    }

    public bool TryBuildTank(UnitKind kind)
    {
        int cost = kind == UnitKind.HeavyTank ? 280 : kind == UnitKind.Harvester ? 140 : 170;
        if (Supplies < cost) return false;
        var baseUnit = Units.FirstOrDefault(u => u.Alive && u.Faction == FactionKind.Player && u.Kind == UnitKind.CommandCenter);
        if (baseUnit == null) return false;
        Supplies -= cost;
        AddTank(FactionKind.Player, 0, baseUnit.Position + Scatter(260), kind);
        return true;
    }
}
