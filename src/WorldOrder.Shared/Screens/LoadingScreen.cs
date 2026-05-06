using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WorldOrder.Core;
using WorldOrder.World;

namespace WorldOrder.Screens;

public sealed class LoadingScreen : GameScreen
{
    private readonly WorldState? _newState;
    private readonly string? _saveFolder;
    private WorldSession? _session;
    private int _step;
    private string _status = "STARTING";

    public LoadingScreen(GameRoot game, WorldState? newState, string? saveFolder) : base(game)
    {
        _newState = newState;
        _saveFolder = saveFolder;
    }

    public override void Update(GameTime gameTime)
    {
        switch (_step)
        {
            case 0:
                _status = _saveFolder is null ? "CREATING WORLD STATE" : "READING SAVE";
                var state = _saveFolder is null ? _newState! : WorldSaveSystem.Load(_saveFolder);
                _session = new WorldSession(Game, state);
                _step++;
                break;
            case 1:
                _status = "GENERATING SPAWN REGION";
                _session!.Preload();
                _step++;
                break;
            case 2:
                _status = "SPAWNING SURVIVOR";
                _session!.Chunks.EnsureAround(_session.Player.Position);
                _step++;
                break;
            case 3:
                _status = "SAVING WORLD";
                _session!.SaveNow(false);
                _step++;
                break;
            default:
                Game.Camera.Snap(_session!.Player.Position);
                Game.Screens.Change(new PlayScreen(Game, _session));
                break;
        }
    }

    public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        Game.GraphicsDevice.Clear(Balance.ClearColor);
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
        var w = Game.GraphicsDevice.Viewport.Width;
        var h = Game.GraphicsDevice.Viewport.Height;
        Game.Ui.Label(spriteBatch, "WORLD ORDER", new Vector2(w / 2 - 190, h / 2 - 120), new Color(236, 220, 150), 5);
        Game.Ui.Label(spriteBatch, _status, new Vector2(w / 2 - 190, h / 2 - 30), Color.White, 2);
        var bar = new Rectangle(w / 2 - 220, h / 2 + 24, 440, 24);
        Game.Ui.Bar(spriteBatch, bar, MathHelper.Clamp(_step / 4f, 0f, 1f), new Color(210, 171, 75), new Color(35, 38, 36));
        spriteBatch.End();
    }
}
