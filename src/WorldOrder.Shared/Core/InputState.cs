using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

namespace WorldOrder.Core;

public sealed class InputState
{
    private KeyboardState _keyboard;
    private KeyboardState _previousKeyboard;
    private MouseState _mouse;
    private MouseState _previousMouse;
    private TouchCollection _touches;
    private TouchCollection _previousTouches;

    public Vector2 PointerScreen { get; private set; }
    public int ScrollDelta => _mouse.ScrollWheelValue - _previousMouse.ScrollWheelValue;
    public bool HasTouch => _touches.Count > 0;

    public void Update()
    {
        _previousKeyboard = _keyboard;
        _previousMouse = _mouse;
        _previousTouches = _touches;
        _keyboard = Keyboard.GetState();
        _mouse = Mouse.GetState();
        _touches = TouchPanel.GetState();
        PointerScreen = _touches.Count > 0 ? _touches[0].Position : new Vector2(_mouse.X, _mouse.Y);
    }

    public bool Down(Keys key) => _keyboard.IsKeyDown(key);
    public bool Pressed(Keys key) => _keyboard.IsKeyDown(key) && !_previousKeyboard.IsKeyDown(key);
    public bool Released(Keys key) => !_keyboard.IsKeyDown(key) && _previousKeyboard.IsKeyDown(key);
    public bool LeftClick => _mouse.LeftButton == ButtonState.Pressed && _previousMouse.LeftButton == ButtonState.Released;
    public bool LeftDown => _mouse.LeftButton == ButtonState.Pressed || AnyTouchPressed();
    public bool RightClick => _mouse.RightButton == ButtonState.Pressed && _previousMouse.RightButton == ButtonState.Released;
    public bool Accept => Pressed(Keys.Enter) || Pressed(Keys.Space) || Pressed(Keys.E) || LeftClick;
    public bool Cancel => Pressed(Keys.Escape) || Pressed(Keys.Back);

    public Vector2 Movement
    {
        get
        {
            var movement = Vector2.Zero;
            if (Down(Keys.W) || Down(Keys.Up)) movement.Y -= 1f;
            if (Down(Keys.S) || Down(Keys.Down)) movement.Y += 1f;
            if (Down(Keys.A) || Down(Keys.Left)) movement.X -= 1f;
            if (Down(Keys.D) || Down(Keys.Right)) movement.X += 1f;
            if (_touches.Count > 0)
            {
                var p = _touches[0].Position;
                var origin = new Vector2(170f, 560f);
                if (Vector2.DistanceSquared(p, origin) < 180f * 180f)
                {
                    movement = MathTools.SafeNormalize(p - origin);
                }
            }
            return MathTools.SafeNormalize(movement);
        }
    }

    public Vector2 WorldPointer(Camera2D camera, GraphicsDevice graphicsDevice) => camera.ScreenToWorld(graphicsDevice, PointerScreen);

    private bool AnyTouchPressed()
    {
        foreach (var touch in _touches)
        {
            if (touch.State == TouchLocationState.Pressed || touch.State == TouchLocationState.Moved) return true;
        }
        return false;
    }
}
