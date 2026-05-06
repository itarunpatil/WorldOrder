using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using WorldOrder.Core;
using WorldOrder.Rendering;
using WorldOrder.Gameplay;

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
        var viewport = Game.GraphicsDevice.Viewport.Bounds;
        for (var i = 0; i < Inventory.HotbarOrder.Length; i++)
        {
            if (Game.Input.Tapped(HudRenderer.HotbarSlotRect(viewport, i)))
            {
                _session.SelectedHotbarIndex = i;
                return;
            }
        }

        if (Game.Input.Pressed(Keys.I) || Game.Input.Pressed(Keys.C) || Game.Input.Tapped(TouchLayout.Inventory(viewport)))
        {
            _session.CraftingOpen = !_session.CraftingOpen;
            _session.BuildMode = false;
        }

        if (_session.CraftingOpen)
        {
            if (Game.Input.Pressed(Keys.Escape) || Game.Input.Tapped(TouchLayout.Pause(viewport))) _session.CraftingOpen = false;
            HandleCraftingInput(viewport);
            Game.Camera.Follow(_session.Player.Position, (float)gameTime.ElapsedGameTime.TotalSeconds);
            return;
        }

        if (Game.Input.Pressed(Keys.Escape) || Game.Input.Tapped(TouchLayout.Pause(viewport))) _paused = !_paused;
        if (_paused)
        {
            if (Game.Input.Pressed(Keys.M)) { _session.SaveNow(); Game.Screens.Change(new MainMenuScreen(Game)); }
            if (Game.Input.Tapped(new Rectangle(viewport.Width / 2 - 170, viewport.Height / 2 + 54, 340, 46))) { _session.SaveNow(); Game.Screens.Change(new MainMenuScreen(Game)); }
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
            var rect = new Rectangle(Game.GraphicsDevice.Viewport.Width / 2 - 230, Game.GraphicsDevice.Viewport.Height / 2 - 120, 460, 250);
            Game.Ui.Panel(spriteBatch, rect, new Color(225, 188, 84), new Color(12, 14, 14, 235));
            Game.Ui.Label(spriteBatch, "PAUSED", new Vector2(rect.X + 118, rect.Y + 36), new Color(236, 220, 150), 5);
            Game.Ui.Label(spriteBatch, "ESC / PAUSE BUTTON RESUME", new Vector2(rect.X + 70, rect.Y + 118), Color.White, 2);
            var menuRect = new Rectangle(rect.X + 60, rect.Y + 174, rect.Width - 120, 46);
            Game.Ui.Button(spriteBatch, menuRect, "SAVE AND MENU", false);
        }
        spriteBatch.End();
    }

    private void HandleCraftingInput(Rectangle viewport)
    {
        var keys = new[] { Keys.D1, Keys.D2, Keys.D3, Keys.D4, Keys.D5, Keys.NumPad1, Keys.NumPad2, Keys.NumPad3, Keys.NumPad4, Keys.NumPad5 };
        for (var i = 0; i < GameDefinitions.Recipes.Length; i++)
        {
            if ((i < 5 && (Game.Input.Pressed(keys[i]) || Game.Input.Pressed(keys[i + 5]))) || Game.Input.Tapped(HudRenderer.CraftingRecipeRect(viewport, i)))
            {
                _session.TryCraftRecipe(i);
                return;
            }
        }
    }

}
