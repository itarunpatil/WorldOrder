using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using WorldOrder.Core;

namespace WorldOrder.Screens;

public sealed class MainMenuScreen : GameScreen
{
    private readonly string[] _items = { "CREATE NEW WORLD", "LOAD WORLD", "SETTINGS", "CREDITS", "QUIT" };
    private int _selected;

    public MainMenuScreen(GameRoot game) : base(game) { }

    public override void Update(GameTime gameTime)
    {
        if (Game.Input.Pressed(Keys.Down) || Game.Input.Pressed(Keys.S)) _selected = (_selected + 1) % _items.Length;
        if (Game.Input.Pressed(Keys.Up) || Game.Input.Pressed(Keys.W)) _selected = (_selected - 1 + _items.Length) % _items.Length;
        if (Game.Input.Accept)
        {
            switch (_selected)
            {
                case 0: Game.Screens.Change(new WorldCreateScreen(Game)); break;
                case 1: Game.Screens.Change(new WorldLoadScreen(Game)); break;
                case 2: Game.Screens.Change(new SimpleTextScreen(Game, "SETTINGS", "THIS PHASE SHIPS FIXED 1280X720 LOGICAL UI, POINT FILTERING, AUTOSAVE, AND RAW ASSET IMPORT.\nPRESS ESC TO RETURN.")); break;
                case 3: Game.Screens.Change(new SimpleTextScreen(Game, "CREDITS", "WORLD ORDER\nCODE AND PROCEDURAL FALLBACK ART GENERATED FOR THIS REPOSITORY.\nOPTIONAL POST APOCALYPSE ASSET PACK BY THELAZYSTONE CAN BE IMPORTED LOCALLY.\nPRESS ESC TO RETURN.")); break;
                case 4: Game.Exit(); break;
            }
        }
    }

    public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        var gd = Game.GraphicsDevice;
        gd.Clear(Balance.ClearColor);
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
        DrawBackground(spriteBatch);
        Game.Ui.Label(spriteBatch, "WORLD ORDER", new Vector2(84, 78), new Color(236, 220, 150), 7);
        Game.Ui.Label(spriteBatch, "POST APOCALYPSE ZOMBIE SURVIVOR", new Vector2(96, 150), new Color(177, 185, 170), 2);
        var startY = 240;
        for (var i = 0; i < _items.Length; i++)
        {
            Game.Ui.Button(spriteBatch, new Rectangle(100, startY + i * 72, 430, 54), _items[i], i == _selected);
        }
        var status = Game.Art.ExternalArtLoaded ? "ASSET PACK: IMPORTED" : "ASSET PACK: FALLBACK ART ACTIVE";
        Game.Ui.Label(spriteBatch, status, new Vector2(96, gd.Viewport.Height - 58), new Color(196, 199, 185), 2);
        Game.Ui.Label(spriteBatch, "ARROWS/WASD + ENTER", new Vector2(96, gd.Viewport.Height - 30), new Color(142, 150, 140), 1);
        spriteBatch.End();
    }

    private void DrawBackground(SpriteBatch batch)
    {
        var w = Game.GraphicsDevice.Viewport.Width;
        var h = Game.GraphicsDevice.Viewport.Height;
        batch.Draw(Game.Art.Pixel, new Rectangle(0, 0, w, h), new Color(18, 22, 20));
        for (var i = 0; i < 22; i++)
        {
            var x = (i * 97) % w;
            var y = 210 + (i * 43) % (h - 210);
            var rect = new Rectangle(x, y, 120 + i % 5 * 30, 18 + i % 4 * 11);
            batch.Draw(Game.Art.Pixel, rect, new Color(28 + i % 3 * 8, 32, 30) * 0.85f);
        }
    }
}
