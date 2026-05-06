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
    private Task<string>? _mobileKeyboardTask;
    private string? _keyboardError;
    private float _caretTimer;

    private const string MobileKeyboardRows = "QWERTYUIOP|ASDFGHJKL|ZXCVBNM";

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

        if (HandleMobileNamePad(layout)) return;

        if (_mobileKeyboardTask is not null && !_mobileKeyboardTask.IsCompleted) return;

        if (Game.Input.Escape)
        {
            if (_editingName) _editingName = false;
            else Game.Screens.Change(new MainMenuScreen(Game));
            return;
        }

        if (Game.Input.Pressed(Keys.Tab))
        {
            _editingName = !_editingName;
            return;
        }

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
        Game.Ui.Panel(spriteBatch, layout.Panel, new Color(95, 100, 92), new Color(18, 20, 19, 230));
        Game.Ui.Label(spriteBatch, "CREATE NEW WORLD", new Vector2(layout.Panel.X + 38, layout.Panel.Y + 38), new Color(236, 220, 150), 5);
        Game.Ui.Label(spriteBatch, "WORLD NAME", new Vector2(layout.Panel.X + 44, layout.Panel.Y + 135), Color.White, 2);
        Game.Ui.Panel(spriteBatch, layout.NameBox, _editingName ? new Color(227, 190, 88) : new Color(90, 94, 88), new Color(10, 12, 12, 230));
        var caret = _editingName && (_caretTimer % 1f) < 0.55f && (_mobileKeyboardTask is null || _mobileKeyboardTask.IsCompleted) ? ">" : string.Empty;
        Game.Ui.Label(spriteBatch, _name + caret, new Vector2(layout.NameBox.X + 14, layout.NameBox.Y + 16), new Color(230, 235, 218), 2);

        Game.Ui.Label(spriteBatch, "SEED", new Vector2(layout.Panel.X + 44, layout.Panel.Y + 248), Color.White, 2);
        Game.Ui.Label(spriteBatch, _seed.ToString(), new Vector2(layout.Panel.X + 44, layout.Panel.Y + 280), new Color(200, 207, 194), 2);
        Game.Ui.Button(spriteBatch, layout.Back, "BACK", false);
        Game.Ui.Button(spriteBatch, layout.Random, "RANDOM", false);
        Game.Ui.Button(spriteBatch, layout.Create, "CREATE", true);

        if (OperatingSystem.IsAndroid() || Game.Input.HasTouch)
        {
            var line = _mobileKeyboardTask is not null && !_mobileKeyboardTask.IsCompleted ? "SYSTEM KEYBOARD OPEN" : "TAP NAME TO OPEN KEYBOARD, OR USE QUICK KEYS BELOW";
            Game.Ui.Label(spriteBatch, line, new Vector2(layout.Panel.X + 44, layout.Panel.Bottom + 14), new Color(194, 202, 188), 2);
            if (!string.IsNullOrWhiteSpace(_keyboardError)) Game.Ui.Label(spriteBatch, _keyboardError, new Vector2(layout.Panel.X + 44, layout.Panel.Bottom + 42), new Color(232, 120, 100), 1);
            DrawMobileNamePad(spriteBatch, layout);
        }
        else
        {
            Game.Ui.Label(spriteBatch, _editingName ? "TYPE NAME  BACKSPACE DELETE  ENTER DONE  ESC UNFOCUS" : "TAB EDIT NAME  R RANDOMIZE  ENTER CREATE  ESC BACK", new Vector2(layout.Panel.X + 44, layout.Panel.Bottom + 14), new Color(194, 202, 188), 2);
        }
        spriteBatch.End();
    }

    private bool HandleMobileNamePad(ScreenLayout layout)
    {
        if (!(OperatingSystem.IsAndroid() || Game.Input.HasTouch)) return false;
        foreach (var key in MobileKeys(layout))
        {
            if (!Game.Input.Tapped(key.Rect)) continue;
            _editingName = true;
            switch (key.Text)
            {
                case "DEL": RemoveLastCharacter(); break;
                case "SPACE": AppendCharacter(' '); break;
                default: AppendCharacter(key.Text[0]); break;
            }
            return true;
        }
        return false;
    }

    private void DrawMobileNamePad(SpriteBatch spriteBatch, ScreenLayout layout)
    {
        foreach (var key in MobileKeys(layout))
        {
            Game.Ui.Button(spriteBatch, key.Rect, key.Text, false);
        }
    }

    private IEnumerable<MobileKey> MobileKeys(ScreenLayout layout)
    {
        var viewport = Game.GraphicsDevice.Viewport.Bounds;
        var keyW = Math.Clamp(viewport.Width / 18, 44, 64);
        var keyH = 38;
        var gap = 6;
        var startY = Math.Min(layout.Panel.Bottom + 74, viewport.Height - 156);
        var rows = MobileKeyboardRows.Split('|');
        for (var row = 0; row < rows.Length; row++)
        {
            var text = rows[row];
            var rowWidth = text.Length * keyW + (text.Length - 1) * gap;
            var x = viewport.Width / 2 - rowWidth / 2;
            for (var i = 0; i < text.Length; i++)
            {
                yield return new MobileKey(text[i].ToString(), new Rectangle(x + i * (keyW + gap), startY + row * (keyH + gap), keyW, keyH));
            }
        }
        yield return new MobileKey("DEL", new Rectangle(viewport.Width / 2 - 170, startY + 3 * (keyH + gap), 96, keyH));
        yield return new MobileKey("SPACE", new Rectangle(viewport.Width / 2 - 62, startY + 3 * (keyH + gap), 170, keyH));
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
        _name = (_name + char.ToUpperInvariant(ch));
    }

    private void RemoveLastCharacter()
    {
        if (_name.Length > 0) _name = _name[..^1];
    }

    private void CreateWorld()
    {
        var state = WorldSaveSystem.CreateNew(_name, _seed);
        Game.Screens.Change(new LoadingScreen(Game, state, null));
    }

    private ScreenLayout Layout()
    {
        var viewport = Game.GraphicsDevice.Viewport.Bounds;
        var panelW = Math.Min(760, viewport.Width - 80);
        var panelH = 430;
        var panel = new Rectangle(viewport.Width / 2 - panelW / 2, 78, panelW, panelH);
        var left = panel.X + 44;
        var nameBox = new Rectangle(left, panel.Y + 166, Math.Min(500, panel.Width - 88), 52);
        var back = new Rectangle(left, panel.Y + 352, 160, 48);
        var random = new Rectangle(back.Right + 20, back.Y, 210, 48);
        var create = new Rectangle(random.Right + 20, back.Y, 230, 48);
        if (create.Right > panel.Right - 32) create = new Rectangle(panel.Right - 292, back.Y, 260, 48);
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
    private readonly record struct MobileKey(string Text, Rectangle Rect);
}
