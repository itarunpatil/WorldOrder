using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace WorldOrder;

public sealed class Game1 : Game
{
    private readonly GraphicsDeviceManager _graphics;
    private SpriteBatch? _spriteBatch;
    private InputState _input = new();

    public AssetBank Assets { get; private set; } = null!;
    public BitmapFont Font { get; private set; } = null!;
    public ScreenStack Screens { get; private set; } = null!;
    public SaveManager Saves { get; private set; } = null!;
    public Random Random { get; } = new(1337);
    public Texture2D Pixel => Assets.Pixel;
    public Vector2 VirtualSize { get; private set; } = new(1280, 720);
    public bool IsMobile { get; }

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
#if ANDROID
        IsMobile = true;
#else
        IsMobile = false;
#endif
    }

    protected override void Initialize()
    {
        Window.Title = "World Order - Phase 1";
        Window.AllowUserResizing = !IsMobile;
        _graphics.SynchronizeWithVerticalRetrace = true;
        _graphics.PreferredBackBufferWidth = 1280;
        _graphics.PreferredBackBufferHeight = 720;
        _graphics.IsFullScreen = IsMobile;
        _graphics.ApplyChanges();
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        Assets = new AssetBank(GraphicsDevice);
        Assets.LoadCore();
        Font = new BitmapFont(Assets.Get("font_hud"), 24, 32);
        Saves = new SaveManager();
        Screens = new ScreenStack(this);
        Screens.Change(new MainMenuScreen(this));
    }

    protected override void Update(GameTime gameTime)
    {
        _input.Update(IsActive);
        if (_input.KeyPressed(Keys.F11) && !IsMobile)
        {
            _graphics.IsFullScreen = !_graphics.IsFullScreen;
            _graphics.ApplyChanges();
        }

        if (_input.KeyPressed(Keys.Escape) && Screens.Current is GameScreen)
        {
            Screens.Change(new WorldSelectScreen(this));
        }

        Screens.Update(gameTime, _input);
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(new Color(28, 24, 20));
        if (_spriteBatch == null)
        {
            return;
        }

        var vp = GraphicsDevice.Viewport;
        VirtualSize = new Vector2(vp.Width, vp.Height);
        Screens.Draw(gameTime, _spriteBatch);
        base.Draw(gameTime);
    }
}
