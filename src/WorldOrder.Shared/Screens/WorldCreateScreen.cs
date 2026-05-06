using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using WorldOrder.Core;
using WorldOrder.World;

namespace WorldOrder.Screens;

public sealed class WorldCreateScreen : GameScreen
{
    private string _name = "ASHFALL";
    private int _seed;
    private bool _editingName = true;
    private int _selectedMap;
    private Task<string>? _mobileKeyboardTask;
    private string? _keyboardError;
    private float _caretTimer;

    public WorldCreateScreen(GameRoot game) : base(game)
    {
        _seed = Hashing.StableStringHash(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString());
    }

    public override void Update(GameTime gameTime)
    {
        _caretTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
        PollMobileKeyboard();
        var layout = Layout();

        if (Game.Input.Tapped(layout.NameBox))
        {
            _editingName = true;
            RequestMobileKeyboard();
            return;
        }

        for (var i = 0; i < WorldMapCatalog.Summaries.Count; i++)
        {
            if (!Game.Input.Tapped(MapCardRect(layout, i))) continue;
            _selectedMap = i;
            _editingName = false;
            return;
        }

        if (Game.Input.Tapped(layout.Back))
        {
            Game.Screens.Change(new MainMenuScreen(Game));
            return;
        }

        if (Game.Input.Tapped(layout.Random))
        {
            _seed = Hashing.StableStringHash(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString());
            return;
        }

        if (Game.Input.Tapped(layout.Create))
        {
            CreateWorld();
            return;
        }

        if (_mobileKeyboardTask is not null && !_mobileKeyboardTask.IsCompleted) return;

        if (Game.Input.Escape)
        {
            if (_editingName) _editingName = false;
            else Game.Screens.Change(new MainMenuScreen(Game));
            return;
        }

        if (Game.Input.Pressed(Keys.Left) || Game.Input.Pressed(Keys.A)) _selectedMap = (_selectedMap - 1 + WorldMapCatalog.Summaries.Count) % WorldMapCatalog.Summaries.Count;
        if (Game.Input.Pressed(Keys.Right) || Game.Input.Pressed(Keys.D)) _selectedMap = (_selectedMap + 1) % WorldMapCatalog.Summaries.Count;
        if (Game.Input.Pressed(Keys.Tab)) _editingName = !_editingName;

        if (!_editingName)
        {
            if (Game.Input.Pressed(Keys.R)) _seed = Hashing.StableStringHash(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString());
            if (Game.Input.Confirm) CreateWorld();
            return;
        }

        foreach (var key in Keyboard.GetState().GetPressedKeys())
        {
            if (!Game.Input.Pressed(key)) continue;
            if (key == Keys.Back)
            {
                RemoveLastCharacter();
                continue;
            }
            if (key == Keys.Enter)
            {
                _editingName = false;
                continue;
            }
            var ch = KeyToChar(key, Game.Input.Down(Keys.LeftShift) || Game.Input.Down(Keys.RightShift));
            if (ch != '\0') AppendCharacter(ch);
        }
    }

    public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        var layout = Layout();
        Game.GraphicsDevice.Clear(Balance.ClearColor);
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
        DrawBackground(spriteBatch);
        Game.Ui.Panel(spriteBatch, layout.Panel, new Color(95, 100, 92), new Color(18, 20, 19, 232));
        CenterLabel(spriteBatch, "CREATE NEW WORLD", new Rectangle(layout.Panel.X, layout.Panel.Y + 30, layout.Panel.Width, 40), new Color(236, 220, 150), 5);

        Game.Ui.Label(spriteBatch, "WORLD NAME", new Vector2(layout.Panel.X + 38, layout.Panel.Y + 108), Color.White, 2);
        Game.Ui.Panel(spriteBatch, layout.NameBox, _editingName ? new Color(227, 190, 88) : new Color(90, 94, 88), new Color(10, 12, 12, 230));
        var caret = _editingName && (_caretTimer % 1f) < 0.55f && (_mobileKeyboardTask is null || _mobileKeyboardTask.IsCompleted) ? "_" : string.Empty;
        Game.Ui.Label(spriteBatch, _name + caret, new Vector2(layout.NameBox.X + 14, layout.NameBox.Y + 16), new Color(230, 235, 218), 2);

        Game.Ui.Label(spriteBatch, "SELECT MAP", new Vector2(layout.Panel.X + 38, layout.Panel.Y + 190), Color.White, 2);
        for (var i = 0; i < WorldMapCatalog.Summaries.Count; i++) DrawMapCard(spriteBatch, layout, i);

        Game.Ui.Label(spriteBatch, $"SEED {_seed}", new Vector2(layout.Panel.X + 38, layout.Panel.Y + 442), new Color(200, 207, 194), 2);
        if (!string.IsNullOrWhiteSpace(_keyboardError)) Game.Ui.Label(spriteBatch, _keyboardError, new Vector2(layout.Panel.X + 38, layout.Panel.Y + 468), new Color(232, 120, 100), 1);

        Game.Ui.Button(spriteBatch, layout.Back, "BACK", false);
        Game.Ui.Button(spriteBatch, layout.Random, "RANDOM SEED", false);
        Game.Ui.Button(spriteBatch, layout.Create, "CREATE", true);
        spriteBatch.End();
    }

    private void DrawBackground(SpriteBatch batch)
    {
        var vp = Game.GraphicsDevice.Viewport.Bounds;
        batch.Draw(Game.Art.Pixel, vp, new Color(13, 17, 16));
        for (var y = 0; y < vp.Height; y += 32)
        {
            for (var x = 0; x < vp.Width; x += 32)
            {
                var tile = ((x / 32 + y / 32) % 7) == 0 ? TileType.DryGrass : ((x / 32 + y / 32) % 11) == 0 ? TileType.Rubble : TileType.Asphalt;
                batch.Draw(Game.Art.Tile(tile, x / 32, y / 32), new Rectangle(x, y, 32, 32), Color.White * 0.14f);
            }
        }
        batch.Draw(Game.Art.Pixel, vp, Color.Black * 0.35f);
    }

    private void DrawMapCard(SpriteBatch batch, ScreenLayout layout, int index)
    {
        var summary = WorldMapCatalog.Summaries[index];
        var rect = MapCardRect(layout, index);
        var selected = index == _selectedMap;
        Game.Ui.Panel(batch, rect, selected ? new Color(236, 202, 94) : new Color(92, 98, 91), selected ? new Color(43, 40, 31, 230) : new Color(20, 23, 22, 220));
        Game.Ui.Label(batch, summary.Name, new Vector2(rect.X + 14, rect.Y + 14), selected ? new Color(248, 238, 170) : new Color(212, 218, 204), 2);
        var line1 = summary.Description.Length > 44 ? summary.Description[..44] : summary.Description;
        var line2 = summary.Description.Length > 44 ? summary.Description[44..Math.Min(summary.Description.Length, 88)] : string.Empty;
        Game.Ui.Label(batch, line1.ToUpperInvariant(), new Vector2(rect.X + 14, rect.Y + 46), new Color(175, 186, 172), 1);
        if (!string.IsNullOrWhiteSpace(line2)) Game.Ui.Label(batch, line2.ToUpperInvariant(), new Vector2(rect.X + 14, rect.Y + 64), new Color(175, 186, 172), 1);
    }

    private Rectangle MapCardRect(ScreenLayout layout, int index)
    {
        var x = layout.Panel.X + 38 + index * ((layout.Panel.Width - 76) / 3);
        var w = (layout.Panel.Width - 96) / 3;
        return new Rectangle(x, layout.Panel.Y + 222, w, 144);
    }

    private void CenterLabel(SpriteBatch batch, string text, Rectangle rect, Color color, int scale)
    {
        var size = Game.Font.Measure(text, scale);
        Game.Font.DrawShadow(batch, text, new Vector2(rect.Center.X - size.X * 0.5f, rect.Center.Y - size.Y * 0.5f), color, scale);
    }

    private void RequestMobileKeyboard()
    {
        if (!OperatingSystem.IsAndroid()) return;
        if (_mobileKeyboardTask is not null && !_mobileKeyboardTask.IsCompleted) return;
        try
        {
            _keyboardError = null;
            _mobileKeyboardTask = KeyboardInput.Show("World Order", "Name your world", _name, false);
        }
        catch (Exception ex)
        {
            _keyboardError = ex.Message.Length > 48 ? ex.Message[..48] : ex.Message;
            _mobileKeyboardTask = null;
        }
    }

    private void PollMobileKeyboard()
    {
        if (_mobileKeyboardTask is null || !_mobileKeyboardTask.IsCompleted) return;
        try
        {
            var result = _mobileKeyboardTask.Result;
            if (!string.IsNullOrWhiteSpace(result)) _name = CleanName(result);
        }
        catch (Exception ex)
        {
            _keyboardError = ex.Message.Length > 48 ? ex.Message[..48] : ex.Message;
        }
        finally
        {
            _mobileKeyboardTask = null;
            _editingName = false;
        }
    }

    private void AppendCharacter(char ch)
    {
        if (_name.Length >= 18) return;
        if (!(char.IsLetterOrDigit(ch) || ch == ' ' || ch == '-' || ch == '_')) return;
        _name = _name + char.ToUpperInvariant(ch);
    }

    private void RemoveLastCharacter()
    {
        if (_name.Length > 0) _name = _name[..^1];
    }

    private void CreateWorld()
    {
        var mapId = WorldMapCatalog.Summaries[_selectedMap].Id;
        var state = WorldSaveSystem.CreateNew(CleanName(_name), _seed, mapId);
        Game.Screens.Change(new LoadingScreen(Game, state, null));
    }

    private ScreenLayout Layout()
    {
        var viewport = Game.GraphicsDevice.Viewport.Bounds;
        var panelW = Math.Min(1030, viewport.Width - 80);
        var panelH = Math.Min(620, viewport.Height - 70);
        var panel = new Rectangle(viewport.Width / 2 - panelW / 2, Math.Max(34, viewport.Height / 2 - panelH / 2), panelW, panelH);
        var left = panel.X + 38;
        var nameBox = new Rectangle(left, panel.Y + 134, Math.Min(520, panel.Width - 76), 52);
        var buttonY = panel.Bottom - 82;
        var back = new Rectangle(left, buttonY, 160, 48);
        var random = new Rectangle(back.Right + 20, buttonY, 250, 48);
        var create = new Rectangle(panel.Right - 268, buttonY, 230, 48);
        return new ScreenLayout(panel, nameBox, back, random, create);
    }

    private static string CleanName(string value)
    {
        var chars = value.Trim().Where(c => char.IsLetterOrDigit(c) || c == ' ' || c == '-' || c == '_').Take(18).ToArray();
        var cleaned = new string(chars).Trim();
        return string.IsNullOrWhiteSpace(cleaned) ? "WORLD" : cleaned.ToUpperInvariant();
    }

    private static char KeyToChar(Keys key, bool shift)
    {
        if (key >= Keys.A && key <= Keys.Z) return (char)('A' + ((int)key - (int)Keys.A));
        if (key >= Keys.D0 && key <= Keys.D9) return (char)('0' + ((int)key - (int)Keys.D0));
        if (key >= Keys.NumPad0 && key <= Keys.NumPad9) return (char)('0' + ((int)key - (int)Keys.NumPad0));
        if (key == Keys.Space) return ' ';
        if (key == Keys.OemMinus) return shift ? '_' : '-';
        return '\0';
    }

    private readonly record struct ScreenLayout(Rectangle Panel, Rectangle NameBox, Rectangle Back, Rectangle Random, Rectangle Create);
}
