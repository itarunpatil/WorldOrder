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
        batch.Begin(samplerState: SamplerState.PointClamp);
        var sand = Game.Assets.Get("terrain_sand_1");
        for (int y = -96; y < vp.Height + 96; y += 96)
        for (int x = -96; x < vp.Width + 96; x += 96)
        {
            int offset = (int)(_time * 8f) % 96;
            batch.Draw(sand, new Rectangle(x - offset, y, 96, 96), Color.White * 0.82f);
        }
        UiKit.Fill(batch, Game.Pixel, new Rectangle(0, 0, vp.Width, vp.Height), new Color(22, 15, 9, 118));
        var road = Game.Assets.Get("terrain_road");
        for (int i = -1; i < vp.Width / 120 + 3; i++)
        {
            batch.Draw(road, new Rectangle(i * 120 - (int)(_time * 24f % 120), vp.Height - 66, 120, 66), Color.White * 0.42f);
        }

        float vehicleScale = MathHelper.Clamp(vp.Height / 1050f, 0.42f, 0.62f);
        var tankPos = new Vector2(116, vp.Height - 138);
        batch.Draw(Game.Assets.Get("hull_player_02"), tankPos, null, Color.White * 0.72f, -0.15f, new Vector2(128, 128), vehicleScale, SpriteEffects.None, 0f);
        batch.Draw(Game.Assets.Get("gun_player_02"), tankPos, null, Color.White * 0.88f, -0.07f, new Vector2(47, 150), vehicleScale, SpriteEffects.None, 0f);
        var boat = Game.Assets.Get("boat_water_3_1_" + (1 + ((int)(_time * 6) % 4)));
        batch.Draw(boat, new Vector2(vp.Width - 172, vp.Height - 128), null, Color.White * 0.62f, 0.05f, new Vector2(64, 64), MathHelper.Clamp(vp.Height / 760f, 0.78f, 1.05f), SpriteEffects.None, 0f);
        batch.End();
    }

    protected static bool Hit(Rectangle r, InputState input) => r.Contains(input.Pointer) && input.PointerReleased;
    public abstract void Draw(GameTime time, SpriteBatch batch);
}

public sealed class MainMenuScreen : MenuScreenBase
{
    public MainMenuScreen(Game1 game) : base(game) { }

    private static Rectangle Panel(Viewport vp) => UiKit.ResponsivePanel(vp.Width, vp.Height, 620, 540, 44);

    private static Rectangle ButtonRect(Rectangle panel, int index)
    {
        int width = Math.Min(380, panel.Width - 130);
        int height = 58;
        int x = panel.Center.X - width / 2;
        int startY = panel.Y + Math.Max(210, (int)(panel.Height * 0.42f));
        return new Rectangle(x, startY + index * 76, width, height);
    }

    public override void Update(GameTime time, InputState input)
    {
        base.Update(time, input);
        var panel = Panel(Game.GraphicsDevice.Viewport);
        if (Hit(ButtonRect(panel, 0), input)) Game.Screens.Change(new WorldSelectScreen(Game));
        if (Hit(ButtonRect(panel, 1), input)) Game.Screens.Change(new SettingsScreen(Game));
        if (Hit(ButtonRect(panel, 2), input)) Game.Screens.Change(new CreditsScreen(Game));
    }

    public override void Draw(GameTime time, SpriteBatch batch)
    {
        DrawBackdrop(batch);
        var vp = Game.GraphicsDevice.Viewport;
        batch.Begin(samplerState: SamplerState.PointClamp);
        var panel = Panel(vp);
        UiKit.PanelBox(Game, batch, panel);
        UiKit.DrawCenteredFitted(Game, batch, "WORLD ORDER", new Rectangle(panel.X + 20, panel.Y + 54, panel.Width - 40, 62), UiKit.Ink, 1.55f);
        UiKit.DrawCenteredFitted(Game, batch, "Phase 1 tactical desert campaign", new Rectangle(panel.X + 20, panel.Y + 120, panel.Width - 40, 38), UiKit.InkDim, 0.66f);
        UiKit.Button(Game, batch, LastInput, ButtonRect(panel, 0), "PLAY", true, 1.0f);
        UiKit.Button(Game, batch, LastInput, ButtonRect(panel, 1), "SETTINGS", true, 0.86f);
        UiKit.Button(Game, batch, LastInput, ButtonRect(panel, 2), "CREDITS", true, 0.86f);
        UiKit.DrawCenteredFitted(Game, batch, "Mouse: select / move / attack     Android: tap controls", new Rectangle(panel.X + 20, panel.Bottom - 48, panel.Width - 40, 30), UiKit.InkDim, 0.52f);
        batch.End();
    }
}

public sealed class WorldSelectScreen : MenuScreenBase
{
    public WorldSelectScreen(Game1 game) : base(game) { }

    private static Rectangle Panel(Viewport vp) => UiKit.ResponsivePanel(vp.Width, vp.Height, 960, 620, 36);
    private static Rectangle CreateButton(Rectangle panel) => new(panel.Center.X - 220, panel.Bottom - 76, 210, 56);
    private static Rectangle BackButton(Rectangle panel) => new(panel.Center.X + 10, panel.Bottom - 76, 210, 56);
    private static Rectangle RowRect(Rectangle panel, int index) => new(panel.X + 54, panel.Y + 132 + index * 76, panel.Width - 108, 62);

    public override void Update(GameTime time, InputState input)
    {
        base.Update(time, input);
        if (input.KeyPressed(Keys.Escape)) Game.Screens.Change(new MainMenuScreen(Game));
        var panel = Panel(Game.GraphicsDevice.Viewport);
        int visibleRows = Math.Min(5, Math.Max(1, (panel.Bottom - 220 - (panel.Y + 132)) / 76));
        for (int i = 0; i < Game.Saves.Index.Worlds.Count && i < visibleRows; i++)
        {
            var r = RowRect(panel, i);
            if (r.Contains(input.Pointer) && input.PointerReleased)
            {
                var w = Game.Saves.Index.Worlds[i];
                Game.Saves.Touch(w);
                Game.Screens.Change(new LoadingScreen(Game, w));
            }
        }
        if (Hit(CreateButton(panel), input)) Game.Screens.Change(new WorldCreateScreen(Game));
        if (Hit(BackButton(panel), input)) Game.Screens.Change(new MainMenuScreen(Game));
    }

    public override void Draw(GameTime time, SpriteBatch batch)
    {
        DrawBackdrop(batch);
        var vp = Game.GraphicsDevice.Viewport;
        batch.Begin(samplerState: SamplerState.PointClamp);
        var panel = Panel(vp);
        UiKit.PanelBox(Game, batch, panel, "Worlds");
        UiKit.DrawCenteredFitted(Game, batch, "SELECT WORLD", new Rectangle(panel.X + 20, panel.Y + 48, panel.Width - 40, 46), UiKit.Ink, 1.08f);
        int visibleRows = Math.Min(5, Math.Max(1, (panel.Bottom - 220 - (panel.Y + 132)) / 76));
        if (Game.Saves.Index.Worlds.Count == 0)
        {
            UiKit.DrawCenteredFitted(Game, batch, "No created worlds yet", new Rectangle(panel.X + 40, panel.Y + panel.Height / 2 - 45, panel.Width - 80, 45), UiKit.Ink, 0.88f);
            UiKit.DrawCenteredFitted(Game, batch, "Create a tactical desert campaign to begin.", new Rectangle(panel.X + 40, panel.Y + panel.Height / 2 + 8, panel.Width - 80, 38), UiKit.InkDim, 0.62f);
        }
        else
        {
            for (int i = 0; i < Game.Saves.Index.Worlds.Count && i < visibleRows; i++)
            {
                var w = Game.Saves.Index.Worlds[i];
                var r = RowRect(panel, i);
                bool hover = r.Contains(LastInput.Pointer);
                UiKit.Fill(batch, Game.Pixel, r, hover ? new Color(70, 56, 39, 240) : new Color(43, 36, 29, 226));
                UiKit.Outline(batch, Game.Pixel, r, hover ? UiKit.Accent : new Color(104, 81, 50), 2);
                UiKit.DrawFitted(Game, batch, w.Name, new Vector2(r.X + 18, r.Y + 9), UiKit.Ink, 0.76f, r.Width - 36);
                UiKit.DrawFitted(Game, batch, $"Seed {w.Seed} | {w.Preset} | {w.Difficulty} | Enemies {w.EnemyFactions} Allies {w.AllyFactions}", new Vector2(r.X + 18, r.Y + 36), UiKit.InkDim, 0.48f, r.Width - 36);
            }
        }
        UiKit.Button(Game, batch, LastInput, CreateButton(panel), "CREATE NEW", true, 0.68f);
        UiKit.Button(Game, batch, LastInput, BackButton(panel), "BACK", true, 0.74f);
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

    private static Rectangle Panel(Viewport vp) => UiKit.ResponsivePanel(vp.Width, vp.Height, 980, 640, 32);
    private static int RowY(Rectangle panel, int index) => panel.Y + 134 + index * 58;
    private static int RowX(Rectangle panel) => panel.X + 58;
    private static Rectangle CreateButton(Rectangle panel) => new(panel.Center.X - 220, panel.Bottom - 74, 210, 56);
    private static Rectangle BackButton(Rectangle panel) => new(panel.Center.X + 10, panel.Bottom - 74, 210, 56);
    private static Rectangle LeftStep(Rectangle panel, int row) => new(RowX(panel) + 260, RowY(panel, row), 54, 42);
    private static Rectangle RightStep(Rectangle panel, int row) => new(RowX(panel) + 640, RowY(panel, row), 54, 42);

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

        var panel = Panel(Game.GraphicsDevice.Viewport);
        if (Hit(LeftStep(panel, 2), input)) _difficulty = (Difficulty)(((int)_difficulty + 2) % 3);
        if (Hit(RightStep(panel, 2), input)) _difficulty = (Difficulty)(((int)_difficulty + 1) % 3);
        if (Hit(LeftStep(panel, 3), input)) _preset = (MapPreset)(((int)_preset + 2) % 3);
        if (Hit(RightStep(panel, 3), input)) _preset = (MapPreset)(((int)_preset + 1) % 3);
        if (Hit(LeftStep(panel, 4), input)) _enemies = Math.Clamp(_enemies - 1, 1, 4);
        if (Hit(RightStep(panel, 4), input)) _enemies = Math.Clamp(_enemies + 1, 1, 4);
        if (Hit(LeftStep(panel, 5), input)) _allies = Math.Clamp(_allies - 1, 0, 3);
        if (Hit(RightStep(panel, 5), input)) _allies = Math.Clamp(_allies + 1, 0, 3);
        if (Hit(CreateButton(panel), input)) Create();
        if (Hit(BackButton(panel), input)) Game.Screens.Change(new WorldSelectScreen(Game));
    }

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
        var panel = Panel(vp);
        UiKit.PanelBox(Game, batch, panel, "Create World");
        UiKit.DrawCenteredFitted(Game, batch, "CONFIGURE THE FRONT", new Rectangle(panel.X + 20, panel.Y + 48, panel.Width - 40, 46), UiKit.Ink, 0.95f);
        Field(batch, RowX(panel), RowY(panel, 0), "World name", _name + (_field == 0 ? "_" : ""), _field == 0);
        Field(batch, RowX(panel), RowY(panel, 1), "Seed", _seed + (_field == 1 ? "_" : ""), _field == 1);
        Row(batch, panel, 2, "Difficulty", _difficulty.ToString());
        Row(batch, panel, 3, "Map preset", _preset.ToString());
        Row(batch, panel, 4, "Enemy factions", _enemies.ToString());
        Row(batch, panel, 5, "Ally factions", _allies.ToString());
        UiKit.DrawCenteredFitted(Game, batch, "Tab switches name/seed. Presets generate readable roads, lakes, bases and patrol lanes.", new Rectangle(panel.X + 34, panel.Bottom - 120, panel.Width - 68, 30), UiKit.InkDim, 0.47f);
        UiKit.Button(Game, batch, LastInput, CreateButton(panel), "CREATE", true, 0.74f);
        UiKit.Button(Game, batch, LastInput, BackButton(panel), "BACK", true, 0.74f);
        batch.End();
    }

    private void Field(SpriteBatch batch, int x, int y, string label, string value, bool active)
    {
        Game.Font.Draw(batch, label.ToUpperInvariant(), new Vector2(x, y + 9), UiKit.InkDim, 0.56f);
        var r = new Rectangle(x + 260, y, 430, 42);
        UiKit.Fill(batch, Game.Pixel, r, new Color(31, 27, 23, 242));
        UiKit.Outline(batch, Game.Pixel, r, active ? UiKit.Accent : new Color(91, 70, 45), 2);
        UiKit.DrawFitted(Game, batch, value, new Vector2(r.X + 12, r.Y + 8), UiKit.Ink, 0.58f, r.Width - 24);
    }

    private void Row(SpriteBatch batch, Rectangle panel, int row, string label, string value)
    {
        int x = RowX(panel);
        int y = RowY(panel, row);
        Game.Font.Draw(batch, label.ToUpperInvariant(), new Vector2(x, y + 9), UiKit.InkDim, 0.56f);
        UiKit.Button(Game, batch, LastInput, LeftStep(panel, row), "<", true, 0.62f);
        var r = new Rectangle(x + 330, y, 294, 42);
        UiKit.Fill(batch, Game.Pixel, r, new Color(31, 27, 23, 242));
        UiKit.Outline(batch, Game.Pixel, r, new Color(91, 70, 45), 2);
        UiKit.DrawCenteredFitted(Game, batch, value, r, UiKit.Ink, 0.58f);
        UiKit.Button(Game, batch, LastInput, RightStep(panel, row), ">", true, 0.62f);
    }
}

public sealed class SettingsScreen : MenuScreenBase
{
    public SettingsScreen(Game1 game) : base(game) { }

    private static Rectangle Panel(Viewport vp) => UiKit.ResponsivePanel(vp.Width, vp.Height, 860, 560, 36);
    private static Rectangle BackButton(Rectangle panel) => new(panel.Center.X - 105, panel.Bottom - 76, 210, 56);

    public override void Update(GameTime time, InputState input)
    {
        base.Update(time, input);
        var panel = Panel(Game.GraphicsDevice.Viewport);
        if (input.KeyPressed(Keys.Escape) || Hit(BackButton(panel), input)) Game.Screens.Change(new MainMenuScreen(Game));
    }

    public override void Draw(GameTime time, SpriteBatch batch)
    {
        DrawBackdrop(batch);
        var vp = Game.GraphicsDevice.Viewport;
        batch.Begin(samplerState: SamplerState.PointClamp);
        var panel = Panel(vp);
        UiKit.PanelBox(Game, batch, panel, "Settings");
        Game.Font.Draw(batch, "CONTROLS", new Vector2(panel.X + 44, panel.Y + 86), UiKit.Ink, 0.86f);
        UiKit.DrawFitted(Game, batch, "Desktop: left-drag selects, right-click commands, WASD/arrow keys pan, mouse wheel zooms.", new Vector2(panel.X + 44, panel.Y + 136), UiKit.InkDim, 0.50f, panel.Width - 88);
        UiKit.DrawFitted(Game, batch, "Android: tap to select, tap ground to move, tap enemy to attack, drag screen edges to pan.", new Vector2(panel.X + 44, panel.Y + 178), UiKit.InkDim, 0.50f, panel.Width - 88);
        Game.Font.Draw(batch, "DISPLAY", new Vector2(panel.X + 44, panel.Y + 256), UiKit.Ink, 0.86f);
        UiKit.DrawFitted(Game, batch, "The window is now resizable. UI panels scale to the viewport and gameplay HUD keeps controls inside safe areas.", new Vector2(panel.X + 44, panel.Y + 306), UiKit.InkDim, 0.50f, panel.Width - 88);
        UiKit.Button(Game, batch, LastInput, BackButton(panel), "BACK", true, 0.74f);
        batch.End();
    }
}

public sealed class CreditsScreen : MenuScreenBase
{
    public CreditsScreen(Game1 game) : base(game) { }

    private static Rectangle Panel(Viewport vp) => UiKit.ResponsivePanel(vp.Width, vp.Height, 900, 590, 36);
    private static Rectangle BackButton(Rectangle panel) => new(panel.Center.X - 105, panel.Bottom - 76, 210, 56);

    public override void Update(GameTime time, InputState input)
    {
        base.Update(time, input);
        var panel = Panel(Game.GraphicsDevice.Viewport);
        if (input.KeyPressed(Keys.Escape) || Hit(BackButton(panel), input)) Game.Screens.Change(new MainMenuScreen(Game));
    }

    public override void Draw(GameTime time, SpriteBatch batch)
    {
        DrawBackdrop(batch);
        var vp = Game.GraphicsDevice.Viewport;
        batch.Begin(samplerState: SamplerState.PointClamp);
        var panel = Panel(vp);
        UiKit.PanelBox(Game, batch, panel, "Credits");
        UiKit.DrawCenteredFitted(Game, batch, "WORLD ORDER", new Rectangle(panel.X + 20, panel.Y + 62, panel.Width - 40, 50), UiKit.Ink, 1.08f);
        UiKit.DrawFitted(Game, batch, "Code, game design, UI layout, map generation, AI skirmish systems: OpenAI GPT-5.5 Thinking.", new Vector2(panel.X + 42, panel.Y + 148), UiKit.InkDim, 0.50f, panel.Width - 84);
        UiKit.DrawFitted(Game, batch, "Tanks, weapons, boats, explosions and RPG desert set: CraftPix free asset packs.", new Vector2(panel.X + 42, panel.Y + 205), UiKit.InkDim, 0.50f, panel.Width - 84);
        UiKit.DrawFitted(Game, batch, "Desert Top-Down Tileset: Franco Giachetti / LudicArts.com, CC BY 3.0.", new Vector2(panel.X + 42, panel.Y + 262), UiKit.InkDim, 0.50f, panel.Width - 84);
        UiKit.DrawFitted(Game, batch, "Phase 1 goal: a playable RTS foundation ready for iteration, not a mocked menu shell.", new Vector2(panel.X + 42, panel.Y + 334), UiKit.Ink, 0.54f, panel.Width - 84);
        UiKit.Button(Game, batch, LastInput, BackButton(panel), "BACK", true, 0.74f);
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
        var sand = _game.Assets.Get("terrain_sand_2");
        for (int y = 0; y < vp.Height; y += 96)
        for (int x = 0; x < vp.Width; x += 96)
        {
            batch.Draw(sand, new Rectangle(x, y, 96, 96), Color.White * 0.32f);
        }
        UiKit.DrawCenteredFitted(_game, batch, "GENERATING WORLD", new Rectangle(0, vp.Height / 2 - 118, vp.Width, 54), UiKit.Ink, 1.05f);
        int barWidth = Math.Min(680, vp.Width - 120);
        UiKit.ProgressBar(_game, batch, new Rectangle(vp.Width / 2 - barWidth / 2, vp.Height / 2, barWidth, 46), _progress, $"{(int)(_progress * 100)}%  {_status}");
        UiKit.DrawCenteredFitted(_game, batch, _settings.Name, new Rectangle(40, vp.Height / 2 + 70, vp.Width - 80, 40), UiKit.InkDim, 0.58f);
        batch.End();
    }
}
