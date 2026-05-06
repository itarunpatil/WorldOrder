using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WorldOrder.Assets;
using WorldOrder.Core;
using WorldOrder.Screens;
using WorldOrder.UI;

namespace WorldOrder;

public sealed class GameRoot : Game
{
    private readonly GraphicsDeviceManager _graphics;
    private SpriteBatch? _spriteBatch;

    public GameRoot()
    {
        _graphics = new GraphicsDeviceManager(this)
        {
            PreferredBackBufferWidth = Balance.VirtualWidth,
            PreferredBackBufferHeight = Balance.VirtualHeight,
            SynchronizeWithVerticalRetrace = true,
            PreferMultiSampling = false,
            IsFullScreen = false
        };
        IsMouseVisible = true;
        Window.Title = "World Order";
        InactiveSleepTime = TimeSpan.FromMilliseconds(50);
    }

    public InputState Input { get; } = new();
    public ScreenManager Screens { get; } = new();
    public Camera2D Camera { get; } = new();
    public ArtLibrary Art { get; private set; } = null!;
    public PixelFont Font { get; private set; } = null!;
    public UiRenderer Ui { get; private set; } = null!;

    protected override void Initialize()
    {
        base.Initialize();
        TouchPanelCapabilities();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        Art = new ArtLibrary(GraphicsDevice);
        Art.Load();
        Font = new PixelFont(Art);
        Ui = new UiRenderer(Art, Font);
        Screens.Change(new MainMenuScreen(this));
    }

    protected override void Update(GameTime gameTime)
    {
        Input.Update();
        Screens.Update(gameTime);
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        Screens.Draw(gameTime, _spriteBatch!);
        base.Draw(gameTime);
    }

    private static void TouchPanelCapabilities()
    {
        Microsoft.Xna.Framework.Input.Touch.TouchPanel.EnabledGestures = Microsoft.Xna.Framework.Input.Touch.GestureType.None;
    }
}
