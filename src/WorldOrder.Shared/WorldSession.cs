using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using WorldOrder.Core;
using WorldOrder.Entities;
using WorldOrder.Gameplay;
using WorldOrder.World;

namespace WorldOrder;

public sealed class WorldSession
{
    private float _autoSaveTimer = 30f;
    private readonly Queue<string> _messages = new();
    private float _messageTimer;

    public WorldSession(GameRoot game, WorldState state)
    {
        Game = game;
        State = state;
        Generator = new WorldGenerator(state.Seed);
        Chunks = new ChunkManager(Generator, state);
        Player = new Player(state.PlayerPosition);
        Entities = new EntityManager();
    }

    public GameRoot Game { get; }
    public WorldState State { get; }
    public WorldGenerator Generator { get; }
    public ChunkManager Chunks { get; }
    public Player Player { get; }
    public EntityManager Entities { get; }
    public bool BuildMode { get; set; }
    public int SelectedBuildableIndex { get; set; }
    public string CurrentMessage => _messages.Count == 0 ? string.Empty : _messages.Peek();

    public void Preload()
    {
        Chunks.EnsureAround(Player.Position);
    }

    public void Update(GameTime gameTime)
    {
        var dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        AdvanceClock(dt);
        Chunks.EnsureAround(Player.Position);
        Player.Update(this, gameTime);
        Entities.Update(this, gameTime);
        State.PlayerPosition = Player.Position;
        UpdateVitals(dt);
        HandleBuilding();
        UpdateMessages(dt);
        _autoSaveTimer -= dt;
        if (_autoSaveTimer <= 0f)
        {
            _autoSaveTimer = 30f;
            SaveNow(false);
        }
    }

    public void InteractNearestResource()
    {
        if (BuildMode) return;
        var node = Chunks.FindResource(Player.Position, Balance.PlayerInteractionRange);
        if (node is null)
        {
            Log("NOTHING TO GATHER");
            return;
        }
        var destroyed = node.Damage(1);
        Log($"GATHERING {node.Kind}".ToUpperInvariant());
        if (destroyed)
        {
            Chunks.MarkDepleted(node);
            Entities.DropLoot(node.Position, LootTables.FromResource(node, State.Seed));
            Log($"SALVAGED {node.Kind}".ToUpperInvariant());
        }
    }

    public void PlayerAttack()
    {
        var hitCenter = Player.Position + Player.Facing * Balance.PlayerAttackRange;
        Zombie? best = null;
        var bestSq = float.MaxValue;
        foreach (var zombie in Entities.Zombies)
        {
            var d = Vector2.DistanceSquared(zombie.Position, hitCenter);
            if (d < 40f * 40f && d < bestSq)
            {
                bestSq = d;
                best = zombie;
            }
        }
        if (best is not null)
        {
            var damage = State.Inventory.Count(ItemId.Pistol) > 0 && State.Inventory.Remove(ItemId.Ammo, 1) ? 45f : 18f;
            best.Damage(damage);
            if (best.Removed)
            {
                State.Inventory.Add(ItemId.Cloth, 1);
                if (State.Day % 2 == 0) State.Inventory.Add(ItemId.Scrap, 1);
                Log("ZOMBIE DOWN");
            }
            else Log("HIT ZOMBIE");
        }
    }

    public void DamagePlayer(float damage, float infection)
    {
        State.Vitals.Health = Math.Max(0f, State.Vitals.Health - damage);
        State.Vitals.Infection = Math.Min(100f, State.Vitals.Infection + infection);
        Log("BITTEN");
    }

    public void SaveNow(bool showMessage = true)
    {
        WorldSaveSystem.Save(State);
        if (showMessage) Log("WORLD SAVED");
    }

    public void Log(string text)
    {
        if (_messages.Count > 3) _messages.Dequeue();
        _messages.Enqueue(text);
        _messageTimer = 2.2f;
    }

    private void AdvanceClock(float dt)
    {
        State.WorldTimeSeconds += dt;
        if (State.WorldTimeSeconds >= Balance.DayLengthSeconds)
        {
            State.WorldTimeSeconds -= Balance.DayLengthSeconds;
            State.Day++;
            Log($"DAY {State.Day}".ToUpperInvariant());
        }
    }

    private void UpdateVitals(float dt)
    {
        var v = State.Vitals;
        v.Hunger = Math.Max(0f, v.Hunger - dt * 0.018f);
        v.Thirst = Math.Max(0f, v.Thirst - dt * 0.026f);
        if (v.Hunger <= 0f || v.Thirst <= 0f) v.Health = Math.Max(0f, v.Health - dt * 1.8f);
        if (v.Infection > 0f) v.Health = Math.Max(0f, v.Health - dt * v.Infection * 0.0025f);
        if (v.Health <= 0f) Log("YOU DIED - LOAD OR START AGAIN");
    }

    private void HandleBuilding()
    {
        var input = Game.Input;
        if (!BuildMode) return;
        if (input.Pressed(Keys.Tab)) SelectedBuildableIndex = (SelectedBuildableIndex + 1) % GameDefinitions.Buildables.Length;
        if (!input.LeftClick) return;

        var pointer = input.WorldPointer(Game.Camera, Game.GraphicsDevice);
        var tx = MathTools.FloorDiv((int)MathF.Floor(pointer.X), Balance.TileSize);
        var ty = MathTools.FloorDiv((int)MathF.Floor(pointer.Y), Balance.TileSize);
        var def = GameDefinitions.Buildables[MathTools.ClampInt(SelectedBuildableIndex, 0, GameDefinitions.Buildables.Length - 1)];
        if (Vector2.Distance(Player.Position, new Vector2((tx + 0.5f) * Balance.TileSize, (ty + 0.5f) * Balance.TileSize)) > 160f)
        {
            Log("TOO FAR");
            return;
        }
        if (!Chunks.CanPlaceBlock(tx, ty))
        {
            Log("BLOCKED");
            return;
        }
        if (!State.Inventory.Pay(def.Cost))
        {
            Log("MISSING MATERIALS");
            return;
        }
        var key = ChunkManager.BlockKey(tx, ty);
        State.PlacedBlocks[key] = new PlacedBlock { Key = key, Kind = def.Kind, HitPoints = def.HitPoints };
        Log($"BUILT {def.Name}".ToUpperInvariant());
    }

    private void UpdateMessages(float dt)
    {
        if (_messages.Count == 0) return;
        _messageTimer -= dt;
        if (_messageTimer <= 0f)
        {
            _messages.Dequeue();
            _messageTimer = _messages.Count > 0 ? 1.8f : 0f;
        }
    }
}
