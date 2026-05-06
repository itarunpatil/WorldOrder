using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using WorldOrder.Core;
using WorldOrder.World;

namespace WorldOrder.Screens;

public sealed class WorldLoadScreen : GameScreen
{
    private IReadOnlyList<WorldSaveInfo> _saves = Array.Empty<WorldSaveInfo>();
    private int _selected;

    public WorldLoadScreen(GameRoot game) : base(game) { }

    public override void OnEnter()
    {
        _saves = WorldSaveSystem.ListSaves();
        _selected = 0;
    }

    public override void Update(GameTime gameTime)
    {
        var layout = Layout();
        if (Game.Input.Escape || Game.Input.Tapped(layout.Back)) { Game.Screens.Change(new MainMenuScreen(Game)); return; }
        if (_saves.Count == 0)
        {
            if (Game.Input.Confirm || Game.Input.Tapped(layout.Create)) Game.Screens.Change(new WorldCreateScreen(Game));
            return;
        }
        if (Game.Input.Pressed(Keys.Down) || Game.Input.Pressed(Keys.S)) _selected = (_selected + 1) % _saves.Count;
        if (Game.Input.Pressed(Keys.Up) || Game.Input.Pressed(Keys.W)) _selected = (_selected - 1 + _saves.Count) % _saves.Count;
        for (var i = 0; i < Math.Min(_saves.Count, 8); i++)
        {
            var rect = SaveRect(i);
            if (!Game.Input.Tapped(rect)) continue;
            _selected = i;
            LoadSelected();
            return;
        }
        if (Game.Input.Confirm) LoadSelected();
    }

    public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        var layout = Layout();
        Game.GraphicsDevice.Clear(Balance.ClearColor);
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
        Game.Ui.Panel(spriteBatch, layout.Panel, new Color(94, 98, 91), new Color(16, 18, 17, 230));
        Game.Ui.Label(spriteBatch, "LOAD WORLD", new Vector2(layout.Panel.X + 36, layout.Panel.Y + 36), new Color(236, 220, 150), 5);
        if (_saves.Count == 0)
        {
            Game.Ui.Label(spriteBatch, "NO SAVES FOUND", new Vector2(layout.Panel.X + 36, layout.Panel.Y + 160), Color.White, 3);
            Game.Ui.Label(spriteBatch, "ENTER OR TAP CREATE", new Vector2(layout.Panel.X + 36, layout.Panel.Y + 218), new Color(200, 208, 194), 2);
            Game.Ui.Button(spriteBatch, layout.Create, "CREATE WORLD", true);
        }
        else
        {
            for (var i = 0; i < Math.Min(_saves.Count, 8); i++)
            {
                var save = _saves[i];
                var selected = i == _selected;
                var rect = SaveRect(i);
                Game.Ui.Panel(spriteBatch, rect, selected ? new Color(225, 188, 80) : new Color(82, 86, 80), selected ? new Color(50, 48, 36, 235) : new Color(24, 26, 25, 220));
                var map = WorldMapCatalog.Summary(save.MapId).Name;
                Game.Ui.Label(spriteBatch, $"{save.Name}  DAY {save.Day}  {map}", new Vector2(rect.X + 14, rect.Y + 13), Color.White, 2);
            }
        }
        Game.Ui.Button(spriteBatch, layout.Back, "BACK", false);
        Game.Ui.Label(spriteBatch, "ENTER LOAD", new Vector2(layout.Back.Right + 24, layout.Back.Y + 12), new Color(188, 198, 184), 2);
        spriteBatch.End();
    }

    private ScreenLayout Layout()
    {
        var viewport = Game.GraphicsDevice.Viewport.Bounds;
        var panel = new Rectangle(viewport.Width / 2 - 470, 70, 940, Math.Min(560, viewport.Height - 110));
        return new ScreenLayout(panel, new Rectangle(panel.X + 36, panel.Bottom - 50, 180, 44), new Rectangle(panel.X + 36, panel.Y + 270, 320, 48));
    }

    private Rectangle SaveRect(int index)
    {
        var layout = Layout();
        return new Rectangle(layout.Panel.X + 36, layout.Panel.Y + 120 + index * 56, layout.Panel.Width - 72, 44);
    }

    private void LoadSelected()
    {
        if (_saves.Count == 0) return;
        Game.Screens.Change(new LoadingScreen(Game, null, _saves[_selected].Folder));
    }

    private readonly record struct ScreenLayout(Rectangle Panel, Rectangle Back, Rectangle Create);
}
