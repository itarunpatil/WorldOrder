using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using WorldOrder.Assets;
using WorldOrder.Core;
using WorldOrder.Screens;
using WorldOrder.UI;

namespace WorldOrder;

public sealed class GameRoot : Game
{
    private readonly GraphicsDeviceManager _graphics;
    private SpriteBatch? _spriteBatch;
    private bool _wasActiveFullscreen;

    public GameRoot()
    {
        _graphics = new GraphicsDeviceManager(this)
        {
            PreferredBackBufferWidth = Balance.VirtualWidth,
            PreferredBackBufferHeight = Balance.VirtualHeight,
            SynchronizeWithVerticalRetrace = true,
            PreferMultiSampling = false,
            IsFullScreen = OperatingSystem.IsAndroid(),
            SupportedOrientations = DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight
        };
        _graphics.HardwareModeSwitch = false;
        IsMouseVisible = !OperatingSystem.IsAndroid();
        Window.Title = "World Order";
        Window.AllowUserResizing = !OperatingSystem.IsAndroid();
        Window.ClientSizeChanged += (_, _) => Camera.ClampZoom();
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
        if (OperatingSystem.IsAndroid())
        {
            var display = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode;
            _graphics.PreferredBackBufferWidth = Math.Max(display.Width, display.Height);
            _graphics.PreferredBackBufferHeight = Math.Min(display.Width, display.Height);
            _graphics.IsFullScreen = true;
            _graphics.ApplyChanges();
        }
        else
        {
            _graphics.PreferredBackBufferWidth = Balance.VirtualWidth;
            _graphics.PreferredBackBufferHeight = Balance.VirtualHeight;
            _graphics.ApplyChanges();
        }

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
        Input.Update(GraphicsDevice.Viewport, IsActive);
        if (!IsActive)
        {
            base.Update(gameTime);
            return;
        }
        if (Input.Pressed(Keys.F11) || (Input.Pressed(Keys.Enter) && (Input.Down(Keys.LeftAlt) || Input.Down(Keys.RightAlt)))) ToggleFullscreen();
        Screens.Update(gameTime);
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        Screens.Draw(gameTime, _spriteBatch!);
        base.Draw(gameTime);
    }

    public void ToggleFullscreen()
    {
        if (OperatingSystem.IsAndroid()) return;
        _wasActiveFullscreen = !_wasActiveFullscreen;
        _graphics.IsFullScreen = _wasActiveFullscreen;
        if (!_wasActiveFullscreen)
        {
            _graphics.PreferredBackBufferWidth = Balance.VirtualWidth;
            _graphics.PreferredBackBufferHeight = Balance.VirtualHeight;
        }
        _graphics.ApplyChanges();
    }

    private static void TouchPanelCapabilities()
    {
        Microsoft.Xna.Framework.Input.Touch.TouchPanel.EnabledGestures = Microsoft.Xna.Framework.Input.Touch.GestureType.None;
    }
}
