using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

namespace WorldOrder;

public sealed class InputState
{
    private Vector2 _lastTouchPosition;
    private bool _previousPointerSourceWasTouch;

    public KeyboardState Keyboard { get; private set; }
    public KeyboardState PreviousKeyboard { get; private set; }
    public MouseState Mouse { get; private set; }
    public MouseState PreviousMouse { get; private set; }
    public TouchCollection Touches { get; private set; }
    public Vector2 Pointer { get; private set; }
    public Vector2 PreviousPointer { get; private set; }
    public bool PointerDown { get; private set; }
    public bool PointerPressed { get; private set; }
    public bool PointerReleased { get; private set; }
    public bool SecondaryPressed { get; private set; }
    public bool SecondaryReleased { get; private set; }
    public float ScrollDelta { get; private set; }
    public bool HasTouch { get; private set; }
    public bool PointerIsTouch { get; private set; }

    public void Update(bool active)
    {
        PreviousKeyboard = Keyboard;
        PreviousMouse = Mouse;
        PreviousPointer = Pointer;
        bool wasPointerDown = PointerDown;
        bool wasTouchPointer = PointerIsTouch || _previousPointerSourceWasTouch;

        Keyboard = active ? Microsoft.Xna.Framework.Input.Keyboard.GetState() : default;
        Mouse = active ? Microsoft.Xna.Framework.Input.Mouse.GetState() : default;
        Touches = active ? TouchPanel.GetState() : default;

        bool touchDown = Touches.Count > 0;
        bool mouseDown = Mouse.LeftButton == ButtonState.Pressed;
        bool previousMouseDown = PreviousMouse.LeftButton == ButtonState.Pressed;

        HasTouch = touchDown || (wasPointerDown && wasTouchPointer);
        PointerDown = touchDown || mouseDown;

        if (touchDown)
        {
            Pointer = Touches[0].Position;
            _lastTouchPosition = Pointer;
            PointerIsTouch = true;
            _previousPointerSourceWasTouch = true;
        }
        else if (wasPointerDown && wasTouchPointer)
        {
            // Android reports no active touch on the release frame. Keep the final
            // touch coordinate so UI buttons receive a release at the place tapped.
            Pointer = _lastTouchPosition;
            PointerIsTouch = true;
            _previousPointerSourceWasTouch = true;
        }
        else
        {
            Pointer = new Vector2(Mouse.X, Mouse.Y);
            PointerIsTouch = false;
            _previousPointerSourceWasTouch = false;
        }

        PointerPressed = PointerDown && !wasPointerDown;
        PointerReleased = !PointerDown && wasPointerDown;
        SecondaryPressed = !PointerIsTouch && Mouse.RightButton == ButtonState.Pressed && PreviousMouse.RightButton == ButtonState.Released;
        SecondaryReleased = !PointerIsTouch && Mouse.RightButton == ButtonState.Released && PreviousMouse.RightButton == ButtonState.Pressed;
        ScrollDelta = Mouse.ScrollWheelValue - PreviousMouse.ScrollWheelValue;

        if (!touchDown && !wasPointerDown && !previousMouseDown)
        {
            HasTouch = false;
        }
    }

    public bool KeyDown(Keys key) => Keyboard.IsKeyDown(key);
    public bool KeyPressed(Keys key) => Keyboard.IsKeyDown(key) && !PreviousKeyboard.IsKeyDown(key);

    public string ConsumeTextInput()
    {
        string text = string.Empty;
        foreach (var key in Keyboard.GetPressedKeys())
        {
            if (PreviousKeyboard.IsKeyDown(key)) continue;
            bool shift = Keyboard.IsKeyDown(Keys.LeftShift) || Keyboard.IsKeyDown(Keys.RightShift);
            if (key >= Keys.A && key <= Keys.Z)
            {
                var c = (char)('a' + (key - Keys.A));
                text += shift ? char.ToUpperInvariant(c) : c;
            }
            else if (key >= Keys.D0 && key <= Keys.D9)
            {
                text += (char)('0' + (key - Keys.D0));
            }
            else if (key >= Keys.NumPad0 && key <= Keys.NumPad9)
            {
                text += (char)('0' + (key - Keys.NumPad0));
            }
            else if (key == Keys.Space) text += " ";
            else if (key == Keys.OemMinus) text += shift ? "_" : "-";
            else if (key == Keys.OemPeriod) text += ".";
        }
        return text;
    }
}
