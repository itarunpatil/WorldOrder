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
        if (Game.Input.Cancel) { Game.Screens.Change(new MainMenuScreen(Game)); return; }
        if (_saves.Count == 0)
        {
            if (Game.Input.Accept) Game.Screens.Change(new WorldCreateScreen(Game));
            return;
        }
        if (Game.Input.Pressed(Keys.Down) || Game.Input.Pressed(Keys.S)) _selected = (_selected + 1) % _saves.Count;
        if (Game.Input.Pressed(Keys.Up) || Game.Input.Pressed(Keys.W)) _selected = (_selected - 1 + _saves.Count) % _saves.Count;
        if (Game.Input.Accept)
        {
            Game.Screens.Change(new LoadingScreen(Game, null, _saves[_selected].Folder));
        }
    }

    public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        Game.GraphicsDevice.Clear(Balance.ClearColor);
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
        Game.Ui.Panel(spriteBatch, new Rectangle(90, 70, 940, 560), new Color(94, 98, 91), new Color(16, 18, 17, 230));
        Game.Ui.Label(spriteBatch, "LOAD WORLD", new Vector2(126, 106), new Color(236, 220, 150), 5);
        if (_saves.Count == 0)
        {
            Game.Ui.Label(spriteBatch, "NO SAVES FOUND", new Vector2(126, 230), Color.White, 3);
            Game.Ui.Label(spriteBatch, "ENTER TO CREATE A NEW WORLD", new Vector2(126, 288), new Color(200, 208, 194), 2);
        }
        else
        {
            var y = 190;
            for (var i = 0; i < Math.Min(_saves.Count, 8); i++)
            {
                var save = _saves[i];
                var selected = i == _selected;
                var rect = new Rectangle(126, y + i * 56, 820, 44);
                Game.Ui.Panel(spriteBatch, rect, selected ? new Color(225, 188, 80) : new Color(82, 86, 80), selected ? new Color(50, 48, 36, 235) : new Color(24, 26, 25, 220));
                Game.Ui.Label(spriteBatch, $"{save.Name}  DAY {save.Day}  SEED {save.Seed}", new Vector2(rect.X + 14, rect.Y + 13), Color.White, 2);
            }
        }
        Game.Ui.Label(spriteBatch, "ENTER LOAD  ESC BACK", new Vector2(126, 580), new Color(188, 198, 184), 2);
        spriteBatch.End();
    }
}
