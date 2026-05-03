using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace WorldOrder;

public abstract class MenuScreenBase : IGameScreen
{
    protected readonly Game1 Game;
    private float _time;
    protected InputState LastInput { get; private set; } = new();

    protected MenuScreenBase(Game1 game) => Game = game;

    public virtual void Update(GameTime time, InputState input)
    {
        LastInput = input;
        _time += MathEx.Dt(time);
    }

    protected void DrawBackdrop(SpriteBatch batch)
    {
        var vp = Game.GraphicsDevice.Viewport;
        batch.Begin(samplerState: SamplerState.LinearWrap);
        var sand = Game.Assets.Get("tile_5");
        var dest = new Rectangle(0, 0, vp.Width, vp.Height);
        var src = new Rectangle((int)(_time * 10f), (int)(_time * 3f), vp.Width, vp.Height);
        batch.Draw(sand, dest, src, new Color(92, 73, 45));
        batch.End();

        batch.Begin(samplerState: SamplerState.LinearClamp);
        UiKit.Fill(batch, Game.Pixel, new Rectangle(0, 0, vp.Width, vp.Height), new Color(16, 12, 9, 118));
        var road = Game.Assets.Get("road_18");
        for (int i = -1; i < 9; i++)
        {
            batch.Draw(road, new Rectangle(i * 190 - (int)(_time * 30f % 190), vp.Height - 160, 192, 96), Color.White * 0.25f);
        }
        batch.Draw(Game.Assets.Get("hull_player_02"), new Vector2(90 + (float)Math.Sin(_time) * 8, vp.Height - 265), null, Color.White * 0.78f, -0.15f, new Vector2(128, 128), 0.62f, SpriteEffects.None, 0f);
        batch.Draw(Game.Assets.Get("gun_player_02"), new Vector2(90 + (float)Math.Sin(_time) * 8, vp.Height - 265), null, Color.White * 0.9f, -0.07f, new Vector2(47, 150), 0.62f, SpriteEffects.None, 0f);
        batch.Draw(Game.Assets.Get("boat_water_3_1_" + (1 + ((int)(_time * 6) % 4))), new Vector2(vp.Width - 240, vp.Height - 230), null, Color.White * 0.65f, 0.05f, new Vector2(64, 64), 1.05f, SpriteEffects.None, 0f);
        batch.End();
    }

    public abstract void Draw(GameTime time, SpriteBatch batch);
}

public sealed class MainMenuScreen : MenuScreenBase
{
    public MainMenuScreen(Game1 game) : base(game) { }

    public override void Update(GameTime time, InputState input)
    {
        base.Update(time, input);
        var vp = Game.GraphicsDevice.Viewport;
        int bw = 360, bh = 64;
        int x = vp.Width / 2 - bw / 2;
        int y = vp.Height / 2 + 40;
        if (UiHit(new Rectangle(x, y, bw, bh), input)) Game.Screens.Change(new WorldSelectScreen(Game));
        if (UiHit(new Rectangle(x, y + 84, bw, bh), input)) Game.Screens.Change(new SettingsScreen(Game));
        if (UiHit(new Rectangle(x, y + 168, bw, bh), input)) Game.Screens.Change(new CreditsScreen(Game));
    }

    private static bool UiHit(Rectangle r, InputState input) => r.Contains(input.Pointer) && input.PointerReleased;

    public override void Draw(GameTime time, SpriteBatch batch)
    {
        DrawBackdrop(batch);
        var vp = Game.GraphicsDevice.Viewport;
        batch.Begin(samplerState: SamplerState.PointClamp);
        var panel = UiKit.Centered(vp.Width, vp.Height, 620, 560, 10);
        UiKit.PanelBox(Game, batch, panel);
        Game.Font.DrawCentered(batch, "WORLD ORDER", new Rectangle(panel.X, panel.Y + 35, panel.Width, 80), UiKit.Ink, 1.65f);
        Game.Font.DrawCentered(batch, "Phase 1 tactical desert campaign", new Rectangle(panel.X, panel.Y + 112, panel.Width, 40), UiKit.InkDim, 0.72f);
        int bw = 360, bh = 64;
        int x = vp.Width / 2 - bw / 2;
        int y = vp.Height / 2 + 40;
        UiKit.Button(Game, batch, LastInput, new Rectangle(x, y, bw, bh), "PLAY", true, 1.0f);
        UiKit.Button(Game, batch, LastInput, new Rectangle(x, y + 84, bw, bh), "SETTINGS", true, 0.9f);
        UiKit.Button(Game, batch, LastInput, new Rectangle(x, y + 168, bw, bh), "CREDITS", true, 0.9f);
        Game.Font.DrawCentered(batch, "Mouse: select/move/attack   Android: tap controls", new Rectangle(panel.X, panel.Bottom - 55, panel.Width, 30), UiKit.InkDim, 0.62f);
        batch.End();
    }
}

public sealed class WorldSelectScreen : MenuScreenBase
{
    public WorldSelectScreen(Game1 game) : base(game) { }

    public override void Update(GameTime time, InputState input)
    {
        base.Update(time, input);
        if (input.KeyPressed(Keys.Escape)) Game.Screens.Change(new MainMenuScreen(Game));
        var vp = Game.GraphicsDevice.Viewport;
        int left = vp.Width / 2 - 430;
        int y = 170;
        for (int i = 0; i < Game.Saves.Index.Worlds.Count && i < 6; i++)
        {
            var r = new Rectangle(left, y + i * 84, 860, 68);
            if (r.Contains(input.Pointer) && input.PointerReleased)
            {
                var w = Game.Saves.Index.Worlds[i];
                Game.Saves.Touch(w);
                Game.Screens.Change(new LoadingScreen(Game, w));
            }
        }
        if (UiHit(new Rectangle(vp.Width / 2 - 220, vp.Height - 120, 210, 58), input)) Game.Screens.Change(new WorldCreateScreen(Game));
        if (UiHit(new Rectangle(vp.Width / 2 + 10, vp.Height - 120, 210, 58), input)) Game.Screens.Change(new MainMenuScreen(Game));
    }

    private static bool UiHit(Rectangle r, InputState input) => r.Contains(input.Pointer) && input.PointerReleased;

    public override void Draw(GameTime time, SpriteBatch batch)
    {
        DrawBackdrop(batch);
        var vp = Game.GraphicsDevice.Viewport;
        batch.Begin(samplerState: SamplerState.PointClamp);
        var panel = UiKit.Centered(vp.Width, vp.Height, 970, 670, 0);
        UiKit.PanelBox(Game, batch, panel, "Worlds");
        Game.Font.DrawCentered(batch, "SELECT WORLD", new Rectangle(panel.X, panel.Y + 44, panel.Width, 50), UiKit.Ink, 1.15f);
        int left = vp.Width / 2 - 430;
        int y = 170;
        if (Game.Saves.Index.Worlds.Count == 0)
        {
            Game.Font.DrawCentered(batch, "No created worlds yet", new Rectangle(panel.X, panel.Y + 210, panel.Width, 45), UiKit.Ink, 0.95f);
            Game.Font.DrawCentered(batch, "Create a tactical desert campaign to begin.", new Rectangle(panel.X, panel.Y + 260, panel.Width, 40), UiKit.InkDim, 0.7f);
        }
        else
        {
            for (int i = 0; i < Game.Saves.Index.Worlds.Count && i < 6; i++)
            {
                var w = Game.Saves.Index.Worlds[i];
                var r = new Rectangle(left, y + i * 84, 860, 68);
                bool hover = r.Contains(LastInput.Pointer);
                UiKit.Fill(batch, Game.Pixel, r, hover ? new Color(70, 56, 39, 238) : new Color(43, 36, 29, 220));
                UiKit.Outline(batch, Game.Pixel, r, hover ? UiKit.Accent : new Color(104, 81, 50), 2);
                Game.Font.Draw(batch, w.Name, new Vector2(r.X + 18, r.Y + 10), UiKit.Ink, 0.86f);
                Game.Font.Draw(batch, $"Seed {w.Seed}  |  {w.Preset}  |  {w.Difficulty}  |  Enemies {w.EnemyFactions}  Allies {w.AllyFactions}", new Vector2(r.X + 18, r.Y + 39), UiKit.InkDim, 0.55f);
            }
        }
        UiKit.Button(Game, batch, LastInput, new Rectangle(vp.Width / 2 - 220, vp.Height - 120, 210, 58), "CREATE NEW", true, 0.72f);
        UiKit.Button(Game, batch, LastInput, new Rectangle(vp.Width / 2 + 10, vp.Height - 120, 210, 58), "BACK", true, 0.72f);
        batch.End();
    }
}

public sealed class WorldCreateScreen : MenuScreenBase
{
    private string _name = "World Order Alpha";
    private int _seed = 202601;
    private Difficulty _difficulty = Difficulty.Recruit;
    private MapPreset _preset = MapPreset.DesertBasin;
    private int _enemies = 2;
    private int _allies = 1;
    private int _field;

    public WorldCreateScreen(Game1 game) : base(game) { }

    public override void Update(GameTime time, InputState input)
    {
        base.Update(time, input);
        if (input.KeyPressed(Keys.Escape)) Game.Screens.Change(new WorldSelectScreen(Game));
        if (input.KeyPressed(Keys.Tab)) _field = (_field + 1) % 2;
        if (_field == 0)
        {
            _name += input.ConsumeTextInput();
            if (input.KeyPressed(Keys.Back) && _name.Length > 0) _name = _name[..^1];
            if (_name.Length > 28) _name = _name[..28];
        }
        else
        {
            var t = input.ConsumeTextInput();
            foreach (char c in t)
            {
                if (char.IsDigit(c)) _seed = Math.Clamp(_seed * 10 + (c - '0'), 1, 99999999);
            }
            if (input.KeyPressed(Keys.Back)) _seed /= 10;
        }

        var vp = Game.GraphicsDevice.Viewport;
        int x = vp.Width / 2 - 430;
        if (UiHit(new Rectangle(x + 260, 295, 54, 42), input)) _difficulty = (Difficulty)(((int)_difficulty + 2) % 3);
        if (UiHit(new Rectangle(x + 640, 295, 54, 42), input)) _difficulty = (Difficulty)(((int)_difficulty + 1) % 3);
        if (UiHit(new Rectangle(x + 260, 365, 54, 42), input)) _preset = (MapPreset)(((int)_preset + 2) % 3);
        if (UiHit(new Rectangle(x + 640, 365, 54, 42), input)) _preset = (MapPreset)(((int)_preset + 1) % 3);
        if (UiHit(new Rectangle(x + 260, 435, 54, 42), input)) _enemies = Math.Clamp(_enemies - 1, 1, 4);
        if (UiHit(new Rectangle(x + 640, 435, 54, 42), input)) _enemies = Math.Clamp(_enemies + 1, 1, 4);
        if (UiHit(new Rectangle(x + 260, 505, 54, 42), input)) _allies = Math.Clamp(_allies - 1, 0, 3);
        if (UiHit(new Rectangle(x + 640, 505, 54, 42), input)) _allies = Math.Clamp(_allies + 1, 0, 3);
        if (UiHit(new Rectangle(vp.Width / 2 - 220, vp.Height - 115, 210, 58), input)) Create();
        if (UiHit(new Rectangle(vp.Width / 2 + 10, vp.Height - 115, 210, 58), input)) Game.Screens.Change(new WorldSelectScreen(Game));
    }

    private static bool UiHit(Rectangle r, InputState input) => r.Contains(input.Pointer) && input.PointerReleased;

    private void Create()
    {
        var settings = new WorldSettings
        {
            Name = string.IsNullOrWhiteSpace(_name) ? "Unnamed Front" : _name.Trim(),
            Seed = Math.Clamp(_seed, 1, 99999999),
            Difficulty = _difficulty,
            Preset = _preset,
            EnemyFactions = _enemies,
            AllyFactions = _allies
        };
        Game.Saves.Add(settings);
        Game.Screens.Change(new LoadingScreen(Game, settings));
    }

    public override void Draw(GameTime time, SpriteBatch batch)
    {
        DrawBackdrop(batch);
        var vp = Game.GraphicsDevice.Viewport;
        batch.Begin(samplerState: SamplerState.PointClamp);
        var panel = UiKit.Centered(vp.Width, vp.Height, 970, 670, 0);
        UiKit.PanelBox(Game, batch, panel, "Create World");
        Game.Font.DrawCentered(batch, "CONFIGURE THE FRONT", new Rectangle(panel.X, panel.Y + 42, panel.Width, 50), UiKit.Ink, 1.05f);
        int x = vp.Width / 2 - 430;
        Field(batch, x, 180, "World name", _name + (_field == 0 ? "_" : ""), _field == 0);
        Field(batch, x, 235, "Seed", _seed + (_field == 1 ? "_" : ""), _field == 1);
        Row(batch, x, 295, "Difficulty", _difficulty.ToString());
        Row(batch, x, 365, "Map preset", _preset.ToString());
        Row(batch, x, 435, "Enemy factions", _enemies.ToString());
        Row(batch, x, 505, "Ally factions", _allies.ToString());
        UiKit.Button(Game, batch, LastInput, new Rectangle(vp.Width / 2 - 220, vp.Height - 115, 210, 58), "CREATE", true, 0.78f);
        UiKit.Button(Game, batch, LastInput, new Rectangle(vp.Width / 2 + 10, vp.Height - 115, 210, 58), "BACK", true, 0.78f);
        Game.Font.Draw(batch, "Tab switches text fields. Preset maps generate roads, lakes, bases and patrol lanes.", new Vector2(panel.X + 38, panel.Bottom - 70), UiKit.InkDim, 0.54f);
        batch.End();
    }

    private void Field(SpriteBatch batch, int x, int y, string label, string value, bool active)
    {
        Game.Font.Draw(batch, label.ToUpperInvariant(), new Vector2(x, y + 8), UiKit.InkDim, 0.62f);
        var r = new Rectangle(x + 260, y, 430, 44);
        UiKit.Fill(batch, Game.Pixel, r, new Color(31, 27, 23, 240));
        UiKit.Outline(batch, Game.Pixel, r, active ? UiKit.Accent : new Color(91, 70, 45), 2);
        Game.Font.Draw(batch, value, new Vector2(r.X + 12, r.Y + 8), UiKit.Ink, 0.65f);
    }

    private void Row(SpriteBatch batch, int x, int y, string label, string value)
    {
        Game.Font.Draw(batch, label.ToUpperInvariant(), new Vector2(x, y + 8), UiKit.InkDim, 0.62f);
        UiKit.Button(Game, batch, LastInput, new Rectangle(x + 260, y, 54, 42), "<", true, 0.7f);
        var r = new Rectangle(x + 330, y, 294, 42);
        UiKit.Fill(batch, Game.Pixel, r, new Color(31, 27, 23, 240));
        UiKit.Outline(batch, Game.Pixel, r, new Color(91, 70, 45), 2);
        Game.Font.DrawCentered(batch, value, r, UiKit.Ink, 0.66f);
        UiKit.Button(Game, batch, LastInput, new Rectangle(x + 640, y, 54, 42), ">", true, 0.7f);
    }
}

public sealed class SettingsScreen : MenuScreenBase
{
    public SettingsScreen(Game1 game) : base(game) { }
    public override void Update(GameTime time, InputState input)
    {
        base.Update(time, input);
        if (input.KeyPressed(Keys.Escape) || UiHit(new Rectangle(Game.GraphicsDevice.Viewport.Width / 2 - 105, Game.GraphicsDevice.Viewport.Height - 110, 210, 58), input))
            Game.Screens.Change(new MainMenuScreen(Game));
    }
    private static bool UiHit(Rectangle r, InputState input) => r.Contains(input.Pointer) && input.PointerReleased;
    public override void Draw(GameTime time, SpriteBatch batch)
    {
        DrawBackdrop(batch);
        var vp = Game.GraphicsDevice.Viewport;
        batch.Begin(samplerState: SamplerState.PointClamp);
        var panel = UiKit.Centered(vp.Width, vp.Height, 820, 560);
        UiKit.PanelBox(Game, batch, panel, "Settings");
        Game.Font.Draw(batch, "CONTROLS", new Vector2(panel.X + 44, panel.Y + 88), UiKit.Ink, 0.95f);
        Game.Font.Draw(batch, "Desktop: left-drag selects, right-click commands, WASD pans, mouse wheel zooms.", new Vector2(panel.X + 44, panel.Y + 140), UiKit.InkDim, 0.58f);
        Game.Font.Draw(batch, "Android: tap to select, tap ground to move, tap enemy to attack, drag edges to pan.", new Vector2(panel.X + 44, panel.Y + 184), UiKit.InkDim, 0.58f);
        Game.Font.Draw(batch, "RENDERING", new Vector2(panel.X + 44, panel.Y + 260), UiKit.Ink, 0.95f);
        Game.Font.Draw(batch, "The game uses raw PNG loading rather than XNB, so assets work in CI and Android packaging.", new Vector2(panel.X + 44, panel.Y + 312), UiKit.InkDim, 0.58f);
        UiKit.Button(Game, batch, LastInput, new Rectangle(vp.Width / 2 - 105, vp.Height - 110, 210, 58), "BACK", true, 0.78f);
        batch.End();
    }
}

public sealed class CreditsScreen : MenuScreenBase
{
    public CreditsScreen(Game1 game) : base(game) { }
    public override void Update(GameTime time, InputState input)
    {
        base.Update(time, input);
        if (input.KeyPressed(Keys.Escape) || UiHit(new Rectangle(Game.GraphicsDevice.Viewport.Width / 2 - 105, Game.GraphicsDevice.Viewport.Height - 110, 210, 58), input))
            Game.Screens.Change(new MainMenuScreen(Game));
    }
    private static bool UiHit(Rectangle r, InputState input) => r.Contains(input.Pointer) && input.PointerReleased;
    public override void Draw(GameTime time, SpriteBatch batch)
    {
        DrawBackdrop(batch);
        var vp = Game.GraphicsDevice.Viewport;
        batch.Begin(samplerState: SamplerState.PointClamp);
        var panel = UiKit.Centered(vp.Width, vp.Height, 880, 600);
        UiKit.PanelBox(Game, batch, panel, "Credits");
        Game.Font.DrawCentered(batch, "WORLD ORDER", new Rectangle(panel.X, panel.Y + 64, panel.Width, 52), UiKit.Ink, 1.1f);
        Game.Font.Draw(batch, "Code, game design, UI layout, map generation, AI skirmish systems: OpenAI GPT-5.5 Thinking.", new Vector2(panel.X + 42, panel.Y + 160), UiKit.InkDim, 0.58f);
        Game.Font.Draw(batch, "Tanks, weapons, boats, explosions and RPG desert set: CraftPix free asset packs.", new Vector2(panel.X + 42, panel.Y + 215), UiKit.InkDim, 0.58f);
        Game.Font.Draw(batch, "Desert Top-Down Tileset: Franco Giachetti / LudicArts.com, CC BY 3.0.", new Vector2(panel.X + 42, panel.Y + 270), UiKit.InkDim, 0.58f);
        Game.Font.Draw(batch, "Phase 1 goal: a playable RTS foundation ready for iteration, not a mocked menu shell.", new Vector2(panel.X + 42, panel.Y + 345), UiKit.Ink, 0.62f);
        UiKit.Button(Game, batch, LastInput, new Rectangle(vp.Width / 2 - 105, vp.Height - 110, 210, 58), "BACK", true, 0.78f);
        batch.End();
    }
}

public sealed class LoadingScreen : IGameScreen
{
    private readonly Game1 _game;
    private readonly WorldSettings _settings;
    private float _progress;
    private string _status = "Preparing command table";

    public LoadingScreen(Game1 game, WorldSettings settings)
    {
        _game = game;
        _settings = settings;
    }

    public void Update(GameTime time, InputState input)
    {
        float dt = MathEx.Dt(time);
        _progress += dt * 0.55f;
        if (_progress < 0.22f) _status = "Loading tactical doctrine";
        else if (_progress < 0.48f) _status = "Carving desert roads and lakes";
        else if (_progress < 0.73f) _status = "Deploying factions";
        else if (_progress < 0.97f) _status = "Linking command UI";
        else
        {
            var session = RtsSession.Create(_game, _settings);
            _game.Screens.Change(new GameScreen(_game, session));
        }
    }

    public void Draw(GameTime time, SpriteBatch batch)
    {
        var vp = _game.GraphicsDevice.Viewport;
        batch.Begin(samplerState: SamplerState.PointClamp);
        UiKit.Fill(batch, _game.Pixel, new Rectangle(0, 0, vp.Width, vp.Height), new Color(18, 14, 11));
        var sand = _game.Assets.Get("tile_5");
        for (int y = 0; y < vp.Height; y += 256)
            for (int x = 0; x < vp.Width; x += 256)
                batch.Draw(sand, new Rectangle(x, y, 256, 256), Color.White * 0.18f);
        _game.Font.DrawCentered(batch, "GENERATING WORLD", new Rectangle(0, vp.Height / 2 - 120, vp.Width, 60), UiKit.Ink, 1.15f);
        UiKit.ProgressBar(_game, batch, new Rectangle(vp.Width / 2 - 340, vp.Height / 2, 680, 46), _progress, $"{(int)(_progress * 100)}%  {_status}");
        _game.Font.DrawCentered(batch, _settings.Name, new Rectangle(0, vp.Height / 2 + 70, vp.Width, 40), UiKit.InkDim, 0.62f);
        batch.End();
    }
}
