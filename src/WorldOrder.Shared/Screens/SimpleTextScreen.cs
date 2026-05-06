using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WorldOrder.Core;

namespace WorldOrder.Screens;

public sealed class SimpleTextScreen : GameScreen
{
    private readonly string _title;
    private readonly string _body;

    public SimpleTextScreen(GameRoot game, string title, string body) : base(game)
    {
        _title = title;
        _body = body;
    }

    public override void Update(GameTime gameTime)
    {
        var back = BackRect();
        if (Game.Input.Escape || Game.Input.Tapped(back)) Game.Screens.Change(new MainMenuScreen(Game));
    }

    public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        Game.GraphicsDevice.Clear(Balance.ClearColor);
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
        var panel = new Rectangle(90, 80, Game.GraphicsDevice.Viewport.Width - 180, Game.GraphicsDevice.Viewport.Height - 160);
        Game.Ui.Panel(spriteBatch, panel, new Color(92, 96, 90), new Color(18, 20, 20, 230));
        Game.Ui.Label(spriteBatch, _title, new Vector2(panel.X + 34, panel.Y + 36), new Color(235, 219, 148), 5);
        var y = panel.Y + 130;
        foreach (var line in _body.Split('\n'))
        {
            Game.Ui.Label(spriteBatch, line, new Vector2(panel.X + 34, y), new Color(210, 216, 204), 2);
            y += 34;
        }
        Game.Ui.Button(spriteBatch, BackRect(), "BACK", false);
        spriteBatch.End();
    }

    private Rectangle BackRect()
    {
        var viewport = Game.GraphicsDevice.Viewport.Bounds;
        return new Rectangle(124, viewport.Height - 122, 180, 44);
    }
}
