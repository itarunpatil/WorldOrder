using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using WorldOrder.Core;
using WorldOrder.World;

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

        for (var i = 0; i < _items.Length; i++)
        {
            if (!Game.Input.Tapped(MenuButtonRect(i))) continue;
            _selected = i;
            Activate();
            return;
        }

        if (Game.Input.Confirm) Activate();
    }

    public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        var gd = Game.GraphicsDevice;
        gd.Clear(Balance.ClearColor);
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
        DrawBackground(spriteBatch);
        CenterLabel(spriteBatch, "WORLD ORDER", new Rectangle(0, 70, gd.Viewport.Width, 72), new Color(236, 220, 150), 7);
        CenterLabel(spriteBatch, "SURVIVE THE COLLAPSE", new Rectangle(0, 146, gd.Viewport.Width, 34), new Color(177, 185, 170), 2);
        for (var i = 0; i < _items.Length; i++) Game.Ui.Button(spriteBatch, MenuButtonRect(i), _items[i], i == _selected);
        spriteBatch.End();
    }

    private Rectangle MenuButtonRect(int index)
    {
        var width = 430;
        var x = Game.GraphicsDevice.Viewport.Width / 2 - width / 2;
        return new Rectangle(x, 240 + index * 72, width, 54);
    }

    private void Activate()
    {
        switch (_selected)
        {
            case 0: Game.Screens.Change(new WorldCreateScreen(Game)); break;
            case 1: Game.Screens.Change(new WorldLoadScreen(Game)); break;
            case 2: Game.Screens.Change(new SimpleTextScreen(Game, "SETTINGS", "PHASE 5 USES HAND-AUTHORED MAPS, CLEAN INVENTORY, IMPROVED HUD, BETTER PLAYER ATTACK ANIMATION, AND MOBILE INPUT WITHOUT AN IN-GAME KEYBOARD.\nPRESS ESC OR TAP BACK.")); break;
            case 3: Game.Screens.Change(new SimpleTextScreen(Game, "CREDITS", "WORLD ORDER\nENGINE: MONOGAME\nART: INTEGRATED POST-APOCALYPSE ASSET PACK IN THIS PRIVATE REPOSITORY.\nPRESS ESC OR TAP BACK.")); break;
            case 4: Game.Exit(); break;
        }
    }

    private void DrawBackground(SpriteBatch batch)
    {
        var w = Game.GraphicsDevice.Viewport.Width;
        var h = Game.GraphicsDevice.Viewport.Height;
        batch.Draw(Game.Art.Pixel, new Rectangle(0, 0, w, h), new Color(12, 16, 15));
        var horizon = (int)(h * 0.55f);
        for (var y = 0; y < h; y += 32)
        {
            for (var x = 0; x < w; x += 32)
            {
                var tile = y > horizon ? TileType.Asphalt : (x + y) % 96 == 0 ? TileType.Rubble : TileType.DryGrass;
                batch.Draw(Game.Art.Tile(tile, x / 32, y / 32), new Rectangle(x, y, 32, 32), Color.White * 0.20f);
            }
        }
        var car = Game.Art.Texture("car2");
        batch.Draw(car, new Rectangle(w / 2 - 365, horizon - 24, 140, 80), Color.White * 0.36f);
        var tree = Game.Art.Texture("tree3");
        batch.Draw(tree, new Rectangle(w / 2 + 250, horizon - 170, 120, 170), Color.White * 0.30f);
        for (var i = 0; i < 8; i++)
        {
            var rect = new Rectangle(w / 2 - 260 + i * 72, horizon + 10 + (i % 2) * 36, 48, 12);
            batch.Draw(Game.Art.Pixel, rect, new Color(217, 217, 205) * 0.24f);
        }
        batch.Draw(Game.Art.Pixel, new Rectangle(0, 0, w, h), Color.Black * 0.45f);
    }

    private void CenterLabel(SpriteBatch batch, string text, Rectangle rect, Color color, int scale)
    {
        var size = Game.Font.Measure(text, scale);
        Game.Font.DrawShadow(batch, text, new Vector2(rect.Center.X - size.X * 0.5f, rect.Center.Y - size.Y * 0.5f), color, scale);
    }
}
