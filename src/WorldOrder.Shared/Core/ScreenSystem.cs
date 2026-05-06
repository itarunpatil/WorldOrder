using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WorldOrder.Core;

public abstract class GameScreen
{
    protected GameScreen(GameRoot game) => Game = game;
    protected GameRoot Game { get; }
    public virtual void OnEnter() { }
    public virtual void OnExit() { }
    public abstract void Update(GameTime gameTime);
    public abstract void Draw(GameTime gameTime, SpriteBatch spriteBatch);
}

public sealed class ScreenManager
{
    private GameScreen? _current;

    public GameScreen? Current => _current;

    public void Change(GameScreen next)
    {
        _current?.OnExit();
        _current = next;
        _current.OnEnter();
    }

    public void Update(GameTime gameTime) => _current?.Update(gameTime);
    public void Draw(GameTime gameTime, SpriteBatch spriteBatch) => _current?.Draw(gameTime, spriteBatch);
}
