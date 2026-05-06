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

    public WorldCreateScreen(GameRoot game) : base(game)
    {
        _seed = Hashing.StableStringHash(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString());
    }

    public override void Update(GameTime gameTime)
    {
        var viewport = Game.GraphicsDevice.Viewport.Bounds;
        if (Game.Input.Cancel || Game.Input.Tapped(new Rectangle(154, 454, 160, 48))) { Game.Screens.Change(new MainMenuScreen(Game)); return; }
        if (Game.Input.Tapped(new Rectangle(334, 454, 210, 48))) _seed = Hashing.StableStringHash(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString());
        if (Game.Input.Tapped(new Rectangle(564, 454, 260, 48))) { CreateWorld(); return; }

        foreach (var key in Keyboard.GetState().GetPressedKeys())
        {
            if (!Game.Input.Pressed(key)) continue;
            if (key == Keys.Escape) { Game.Screens.Change(new MainMenuScreen(Game)); return; }
            if (key == Keys.Tab) _editingName = !_editingName;
            if (key == Keys.Back && _editingName && _name.Length > 0) _name = _name[..^1];
            if (key == Keys.R) _seed = Hashing.StableStringHash(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString());
            if (key == Keys.Enter) { CreateWorld(); return; }
            if (_editingName)
            {
                var ch = KeyToChar(key);
                if (ch != '\0' && _name.Length < 18) _name += ch;
            }
        }
    }

    public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        Game.GraphicsDevice.Clear(Balance.ClearColor);
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
        Game.Ui.Panel(spriteBatch, new Rectangle(110, 90, 760, 430), new Color(95, 100, 92), new Color(18, 20, 19, 230));
        Game.Ui.Label(spriteBatch, "CREATE NEW WORLD", new Vector2(148, 130), new Color(236, 220, 150), 5);
        Game.Ui.Label(spriteBatch, "WORLD NAME", new Vector2(154, 225), Color.White, 2);
        Game.Ui.Panel(spriteBatch, new Rectangle(154, 256, 500, 48), _editingName ? new Color(227, 190, 88) : new Color(90, 94, 88), new Color(10, 12, 12, 230));
        Game.Ui.Label(spriteBatch, _name + (_editingName && !OperatingSystem.IsAndroid() ? ">" : string.Empty), new Vector2(168, 272), new Color(230, 235, 218), 2);
        Game.Ui.Label(spriteBatch, "SEED", new Vector2(154, 338), Color.White, 2);
        Game.Ui.Label(spriteBatch, _seed.ToString(), new Vector2(154, 370), new Color(200, 207, 194), 2);
        Game.Ui.Button(spriteBatch, new Rectangle(154, 454, 160, 48), "BACK", false);
        Game.Ui.Button(spriteBatch, new Rectangle(334, 454, 210, 48), "RANDOM", false);
        Game.Ui.Button(spriteBatch, new Rectangle(564, 454, 260, 48), "CREATE", true);
        if (!OperatingSystem.IsAndroid()) Game.Ui.Label(spriteBatch, "TAB TO TOGGLE NAME  R RANDOMIZE  ENTER CREATE  ESC BACK", new Vector2(154, 532), new Color(194, 202, 188), 2);
        spriteBatch.End();
    }

    private void CreateWorld()
    {
        var state = WorldSaveSystem.CreateNew(_name, _seed);
        Game.Screens.Change(new LoadingScreen(Game, state, null));
    }

    private static char KeyToChar(Keys key)
    {
        if (key >= Keys.A && key <= Keys.Z) return (char)('A' + ((int)key - (int)Keys.A));
        if (key >= Keys.D0 && key <= Keys.D9) return (char)('0' + ((int)key - (int)Keys.D0));
        if (key == Keys.Space) return ' ';
        if (key == Keys.OemMinus) return '-';
        return '\0';
    }
}
