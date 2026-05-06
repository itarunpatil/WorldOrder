using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using WorldOrder.Core;
using WorldOrder.Rendering;

namespace WorldOrder.Screens;

public sealed class PlayScreen : GameScreen
{
    private readonly WorldSession _session;
    private readonly WorldRenderer _worldRenderer;
    private readonly HudRenderer _hudRenderer;
    private bool _paused;

    public PlayScreen(GameRoot game, WorldSession session) : base(game)
    {
        _session = session;
        _worldRenderer = new WorldRenderer(game);
        _hudRenderer = new HudRenderer(game);
    }

    public override void Update(GameTime gameTime)
    {
        if (Game.Input.Pressed(Keys.Escape)) _paused = !_paused;
        if (_paused)
        {
            if (Game.Input.Pressed(Keys.M)) { _session.SaveNow(); Game.Screens.Change(new MainMenuScreen(Game)); }
            return;
        }
        Game.Camera.AdjustZoom(Game.Input.ScrollDelta);
        _session.Update(gameTime);
        Game.Camera.Follow(_session.Player.Position, (float)gameTime.ElapsedGameTime.TotalSeconds);
    }

    public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        Game.GraphicsDevice.Clear(Balance.ClearColor);
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, transformMatrix: Game.Camera.GetTransform(Game.GraphicsDevice));
        _worldRenderer.DrawWorld(spriteBatch, _session, gameTime);
        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
        _hudRenderer.Draw(spriteBatch, _session);
        if (_paused)
        {
            var rect = new Rectangle(Game.GraphicsDevice.Viewport.Width / 2 - 220, Game.GraphicsDevice.Viewport.Height / 2 - 110, 440, 220);
            Game.Ui.Panel(spriteBatch, rect, new Color(225, 188, 84), new Color(12, 14, 14, 235));
            Game.Ui.Label(spriteBatch, "PAUSED", new Vector2(rect.X + 120, rect.Y + 42), new Color(236, 220, 150), 5);
            Game.Ui.Label(spriteBatch, "ESC RESUME", new Vector2(rect.X + 96, rect.Y + 124), Color.White, 2);
            Game.Ui.Label(spriteBatch, "M SAVE AND MENU", new Vector2(rect.X + 96, rect.Y + 158), Color.White, 2);
        }
        spriteBatch.End();
    }
}
