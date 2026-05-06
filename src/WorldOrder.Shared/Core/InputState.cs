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
    private Rectangle _viewport = new(0, 0, Balance.VirtualWidth, Balance.VirtualHeight);
    private bool _activeThisFrame = true;

    public Vector2 PointerScreen { get; private set; }
    public int ScrollDelta => _activeThisFrame ? _mouse.ScrollWheelValue - _previousMouse.ScrollWheelValue : 0;
    public bool HasTouch => _touches.Count > 0;
    public bool TouchPressed => _activeThisFrame && AnyTouchPressed();
    public bool PointerPressed => _activeThisFrame && (LeftClick || TouchPressed);
    public bool PointerHeld => _activeThisFrame && (_mouse.LeftButton == ButtonState.Pressed || AnyTouchHeld());

    public void Update(Viewport viewport, bool isActive = true)
    {
        _viewport = viewport.Bounds;
        _activeThisFrame = isActive;

        if (!isActive)
        {
            _keyboard = Keyboard.GetState();
            _previousKeyboard = _keyboard;
            _mouse = Mouse.GetState();
            _previousMouse = _mouse;
            _touches = TouchPanel.GetState();
            _previousTouches = _touches;
            PointerScreen = new Vector2(_mouse.X, _mouse.Y);
            return;
        }

        _previousKeyboard = _keyboard;
        _previousMouse = _mouse;
        _previousTouches = _touches;
        _keyboard = Keyboard.GetState();
        _mouse = Mouse.GetState();
        _touches = TouchPanel.GetState();
        PointerScreen = _touches.Count > 0 ? _touches[0].Position : new Vector2(_mouse.X, _mouse.Y);
    }

    public bool Down(Keys key) => _activeThisFrame && _keyboard.IsKeyDown(key);
    public bool Pressed(Keys key) => _activeThisFrame && _keyboard.IsKeyDown(key) && !_previousKeyboard.IsKeyDown(key);
    public bool Released(Keys key) => _activeThisFrame && !_keyboard.IsKeyDown(key) && _previousKeyboard.IsKeyDown(key);
    public bool LeftClick => _activeThisFrame && _mouse.LeftButton == ButtonState.Pressed && _previousMouse.LeftButton == ButtonState.Released && _viewport.Contains(_mouse.Position);
    public bool LeftDown => _activeThisFrame && _mouse.LeftButton == ButtonState.Pressed && _viewport.Contains(_mouse.Position);
    public bool RightClick => _activeThisFrame && _mouse.RightButton == ButtonState.Pressed && _previousMouse.RightButton == ButtonState.Released && _viewport.Contains(_mouse.Position);

    public bool Accept => Pressed(Keys.Enter) || Pressed(Keys.Space);
    public bool Confirm => Pressed(Keys.Enter) || Pressed(Keys.Space);
    public bool Cancel => Pressed(Keys.Escape) || Pressed(Keys.Back);
    public bool Escape => Pressed(Keys.Escape);

    public Vector2 Movement
    {
        get
        {
            if (!_activeThisFrame) return Vector2.Zero;
            var movement = Vector2.Zero;
            if (Down(Keys.W) || Down(Keys.Up)) movement.Y -= 1f;
            if (Down(Keys.S) || Down(Keys.Down)) movement.Y += 1f;
            if (Down(Keys.A) || Down(Keys.Left)) movement.X -= 1f;
            if (Down(Keys.D) || Down(Keys.Right)) movement.X += 1f;

            var touchMove = TouchMovement();
            if (touchMove.LengthSquared() > 0.01f) movement = touchMove;
            return MathTools.SafeNormalize(movement);
        }
    }

    public bool Tapped(Rectangle rect)
    {
        if (!_activeThisFrame) return false;
        if (LeftClick && rect.Contains(_mouse.Position)) return true;
        foreach (var touch in _touches)
        {
            if (touch.State == TouchLocationState.Pressed && rect.Contains(touch.Position.ToPoint())) return true;
        }
        return false;
    }

    public bool Holding(Rectangle rect)
    {
        if (!_activeThisFrame) return false;
        if (_mouse.LeftButton == ButtonState.Pressed && rect.Contains(_mouse.Position)) return true;
        foreach (var touch in _touches)
        {
            if ((touch.State == TouchLocationState.Pressed || touch.State == TouchLocationState.Moved) && rect.Contains(touch.Position.ToPoint())) return true;
        }
        return false;
    }

    public bool TouchPressedOutsideGameplayControls(Rectangle viewport)
    {
        if (!_activeThisFrame) return false;
        foreach (var touch in _touches)
        {
            if (touch.State == TouchLocationState.Pressed && !TouchLayout.IsGameplayControl(viewport, touch.Position)) return true;
        }
        return false;
    }

    public Vector2 WorldPointer(Camera2D camera, GraphicsDevice graphicsDevice) => camera.ScreenToWorld(graphicsDevice, PointerScreen);

    private Vector2 TouchMovement()
    {
        if (_touches.Count == 0) return Vector2.Zero;
        var movePad = TouchLayout.MovePad(_viewport);
        var origin = TouchLayout.MoveOrigin(_viewport);
        foreach (var touch in _touches)
        {
            if (touch.State is TouchLocationState.Released or TouchLocationState.Invalid) continue;
            if (!movePad.Contains(touch.Position.ToPoint())) continue;
            var delta = touch.Position - origin;
            if (delta.LengthSquared() < 24f * 24f) return Vector2.Zero;
            return MathTools.SafeNormalize(delta);
        }
        return Vector2.Zero;
    }

    private bool AnyTouchPressed()
    {
        foreach (var touch in _touches)
        {
            if (touch.State == TouchLocationState.Pressed) return true;
        }
        return false;
    }

    private bool AnyTouchHeld()
    {
        foreach (var touch in _touches)
        {
            if (touch.State == TouchLocationState.Pressed || touch.State == TouchLocationState.Moved) return true;
        }
        return false;
    }
}
