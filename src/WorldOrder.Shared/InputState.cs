using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

namespace WorldOrder;

public sealed class InputState
{
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

    public void Update(bool active)
    {
        PreviousKeyboard = Keyboard;
        PreviousMouse = Mouse;
        PreviousPointer = Pointer;
        Keyboard = active ? Keyboard.GetState() : default;
        Mouse = active ? Mouse.GetState() : default;
        Touches = active ? TouchPanel.GetState() : default;
        HasTouch = Touches.Count > 0;

        var touchDown = HasTouch;
        Vector2 touchPointer = Pointer;
        if (touchDown)
        {
            touchPointer = Touches[0].Position;
        }

        bool mouseDown = Mouse.LeftButton == ButtonState.Pressed;
        bool prevMouseDown = PreviousMouse.LeftButton == ButtonState.Pressed;
        bool prevPointerDown = PointerDown;

        Pointer = touchDown ? touchPointer : new Vector2(Mouse.X, Mouse.Y);
        PointerDown = touchDown || mouseDown;
        PointerPressed = PointerDown && !prevPointerDown;
        PointerReleased = !PointerDown && prevPointerDown;
        SecondaryPressed = Mouse.RightButton == ButtonState.Pressed && PreviousMouse.RightButton == ButtonState.Released;
        SecondaryReleased = Mouse.RightButton == ButtonState.Released && PreviousMouse.RightButton == ButtonState.Pressed;
        ScrollDelta = Mouse.ScrollWheelValue - PreviousMouse.ScrollWheelValue;
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
