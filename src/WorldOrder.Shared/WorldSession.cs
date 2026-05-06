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
        Generator = new WorldGenerator(state.Seed, state.MapId);
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
    public List<WorldEffect> Effects { get; } = new();
    public bool BuildMode { get; set; }
    public bool CraftingOpen { get; set; }
    public int SelectedBuildableIndex { get; set; }
    public int SelectedHotbarIndex { get; set; }
    public string CurrentMessage => _messages.Count == 0 ? string.Empty : _messages.Peek();

    public void Preload()
    {
        Chunks.EnsureAround(Player.Position);
    }

    public void Update(GameTime gameTime)
    {
        var dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        if (State.Vitals.Health <= 0f)
        {
            UpdateEffects(dt);
            UpdateMessages(dt);
            return;
        }

        AdvanceClock(dt);
        Chunks.EnsureAround(Player.Position);
        Player.Update(this, gameTime);
        Entities.Update(this, gameTime);
        State.PlayerPosition = Player.Position;
        UpdateVitals(dt);
        HandleBuilding();
        UpdateEffects(dt);
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
        EmitEffect(WorldEffectKind.GatherDust, node.Position + new Vector2(0f, -12f), new Vector2(0f, -18f), 0.28f);
        Log($"GATHERING {node.Kind}".ToUpperInvariant());
        if (destroyed)
        {
            Chunks.MarkDepleted(node);
            Entities.DropLoot(node.Position, LootTables.FromResource(node, State.Seed));
            EmitEffect(WorldEffectKind.HitSpark, node.Position, new Vector2(0f, -12f), 0.30f, "+LOOT", new Color(238, 217, 130));
            Log($"SALVAGED {node.Kind}".ToUpperInvariant());
        }
    }

    public void PlayerAttack()
    {
        var attackOrigin = Player.Position + Player.Facing * 20f;
        var bestScore = float.MaxValue;
        Zombie? best = null;
        foreach (var zombie in Entities.Zombies)
        {
            if (zombie.IsDead) continue;
            var toZombie = zombie.Position - Player.Position;
            var distance = toZombie.Length();
            if (distance > Balance.PlayerAttackRange + 32f) continue;
            var direction = MathTools.SafeNormalize(toZombie);
            var facingDot = Vector2.Dot(direction, Player.Facing);
            if (facingDot < 0.18f && distance > 28f) continue;
            var score = distance - facingDot * 24f;
            if (score < bestScore)
            {
                bestScore = score;
                best = zombie;
            }
        }

        EmitEffect(WorldEffectKind.Slash, attackOrigin + Player.Facing * 18f, Player.Facing * 22f, 0.16f);
        if (best is not null)
        {
            var hasPistol = State.Inventory.Count(ItemId.Pistol) > 0;
            var damage = hasPistol && State.Inventory.Remove(ItemId.Ammo, 1) ? 45f : 20f;
            var knockback = MathTools.SafeNormalize(best.Position - Player.Position) * (hasPistol ? 210f : 145f);
            best.Damage(damage, knockback);
            EmitEffect(WorldEffectKind.DamageText, best.Position + new Vector2(0f, -36f), new Vector2(0f, -24f), 0.55f, $"-{(int)damage}", new Color(255, 219, 139));
            EmitEffect(WorldEffectKind.HitSpark, best.Position + new Vector2(0f, -16f), knockback * 0.06f, 0.22f);
            if (best.IsDead)
            {
                var drops = new List<(ItemId Item, int Count)> { (ItemId.Cloth, 1) };
                if (Hashing.Unit((int)best.Position.X, (int)best.Position.Y, State.Seed + State.Day) > 0.55f) drops.Add((ItemId.Scrap, 1));
                if (Hashing.Unit((int)best.Position.X, (int)best.Position.Y, State.Seed + 71) > 0.86f) drops.Add((ItemId.Ammo, 2));
                Entities.DropLoot(best.Position, drops);
                EmitEffect(WorldEffectKind.Blood, best.Position, Vector2.Zero, 12f);
                EmitEffect(WorldEffectKind.DeathPuff, best.Position + new Vector2(0f, -16f), new Vector2(0f, -10f), 0.55f);
                Log("ZOMBIE DOWN");
            }
            else Log("HIT ZOMBIE");
        }
        else
        {
            EmitEffect(WorldEffectKind.DamageText, attackOrigin + Player.Facing * 30f, new Vector2(0f, -16f), 0.35f, "MISS", new Color(180, 185, 174));
        }
    }


    public bool TryCraftRecipe(int index)
    {
        if (index < 0 || index >= GameDefinitions.Recipes.Length) return false;
        var recipe = GameDefinitions.Recipes[index];
        if (!State.Inventory.Pay(recipe.Cost))
        {
            Log("MISSING CRAFTING MATERIALS");
            return false;
        }
        State.Inventory.Add(recipe.Result, recipe.Count);
        EmitEffect(WorldEffectKind.HitSpark, Player.Position + new Vector2(0f, -26f), Vector2.Zero, 0.34f, $"+{recipe.Name.ToUpperInvariant()}", new Color(238, 217, 130));
        Log($"CRAFTED {recipe.Name}".ToUpperInvariant());
        return true;
    }

    public void DamagePlayer(float damage, float infection)
    {
        State.Vitals.Health = Math.Max(0f, State.Vitals.Health - damage);
        State.Vitals.Infection = Math.Min(100f, State.Vitals.Infection + infection);
        EmitEffect(WorldEffectKind.DamageText, Player.Position + new Vector2(0f, -42f), new Vector2(0f, -20f), 0.55f, $"-{(int)damage}", new Color(231, 80, 70));
        Log("BITTEN");
    }

    public void EmitEffect(WorldEffectKind kind, Vector2 position, Vector2 velocity, float lifetime, string? text = null, Color? color = null)
    {
        Effects.Add(new WorldEffect(kind, position, velocity, lifetime, text, color));
        if (Effects.Count > 90) Effects.RemoveRange(0, Effects.Count - 90);
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
        if (v.Health <= 0f)
        {
            v.Health = 0f;
            Log("YOU DIED - LOAD OR START AGAIN");
        }
    }

    private void HandleBuilding()
    {
        var input = Game.Input;
        if (!BuildMode) return;
        if (input.Pressed(Keys.Tab)) SelectedBuildableIndex = (SelectedBuildableIndex + 1) % GameDefinitions.Buildables.Length;
        var viewport = Game.GraphicsDevice.Viewport.Bounds;
        var placePressed = input.LeftClick || input.TouchPressedOutsideGameplayControls(viewport);
        if (!placePressed) return;

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
        EmitEffect(WorldEffectKind.HitSpark, new Vector2((tx + 0.5f) * Balance.TileSize, (ty + 0.5f) * Balance.TileSize), Vector2.Zero, 0.30f, "BUILT", new Color(218, 190, 96));
        Log($"BUILT {def.Name}".ToUpperInvariant());
    }

    private void UpdateEffects(float dt)
    {
        foreach (var effect in Effects) effect.Update(dt);
        Effects.RemoveAll(e => e.Done && e.Kind != WorldEffectKind.Blood);
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
