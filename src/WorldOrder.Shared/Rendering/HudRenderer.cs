using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WorldOrder.Core;
using WorldOrder.Gameplay;
using WorldOrder.UI;

namespace WorldOrder.Rendering;

public sealed class HudRenderer
{
    private readonly GameRoot _game;

    public HudRenderer(GameRoot game) => _game = game;

    public void Draw(SpriteBatch batch, WorldSession session)
    {
        var ui = _game.Ui;
        var viewport = _game.GraphicsDevice.Viewport.Bounds;
        var v = session.State.Vitals;
        ui.Panel(batch, new Rectangle(18, 18, 345, 128), new Color(85, 88, 82), new Color(18, 20, 19, 210));
        ui.Label(batch, $"DAY {session.State.Day}  {ClockText(session)}", new Vector2(32, 32), new Color(232, 226, 188), 2);
        ui.Bar(batch, new Rectangle(32, 60, 160, 16), v.Health / 100f, new Color(178, 48, 43), new Color(55, 27, 27));
        ui.Label(batch, "HP", new Vector2(202, 59), Color.White, 2);
        ui.Bar(batch, new Rectangle(32, 82, 160, 16), v.Hunger / 100f, new Color(202, 138, 62), new Color(55, 37, 24));
        ui.Label(batch, "FOOD", new Vector2(202, 81), Color.White, 2);
        ui.Bar(batch, new Rectangle(32, 104, 160, 16), v.Thirst / 100f, new Color(67, 138, 202), new Color(23, 40, 55));
        ui.Label(batch, "WATER", new Vector2(202, 103), Color.White, 2);
        ui.Bar(batch, new Rectangle(32, 126, 160, 14), v.Stamina / 100f, new Color(72, 179, 88), new Color(22, 48, 27));

        var x = 20;
        var y = viewport.Height - 96;
        ui.Panel(batch, new Rectangle(x, y, 570, 78), new Color(84, 87, 82), new Color(16, 18, 17, 210));
        var inventory = session.State.Inventory;
        DrawItem(batch, "WOOD", inventory.Count(ItemId.Wood), x + 16, y + 18);
        DrawItem(batch, "SCRAP", inventory.Count(ItemId.Scrap), x + 104, y + 18);
        DrawItem(batch, "FOOD", inventory.Count(ItemId.Food), x + 205, y + 18);
        DrawItem(batch, "WATER", inventory.Count(ItemId.Water), x + 294, y + 18);
        DrawItem(batch, "BANDAGE", inventory.Count(ItemId.Bandage), x + 398, y + 18);
        DrawItem(batch, "AMMO", inventory.Count(ItemId.Ammo), x + 510, y + 18);

        if (session.BuildMode)
        {
            var def = GameDefinitions.Buildables[MathTools.ClampInt(session.SelectedBuildableIndex, 0, GameDefinitions.Buildables.Length - 1)];
            ui.Panel(batch, new Rectangle(viewport.Width - 360, 20, 332, 86), new Color(211, 170, 80), new Color(22, 24, 23, 225));
            ui.Label(batch, $"BUILD: {def.Name}".ToUpperInvariant(), new Vector2(viewport.Width - 342, 36), new Color(244, 229, 154), 2);
            ui.Label(batch, "1-4 SELECT  TAB NEXT", new Vector2(viewport.Width - 342, 62), Color.White, 2);
            ui.Label(batch, CostText(def), new Vector2(viewport.Width - 342, 84), new Color(190, 212, 184), 2);
        }

        if (!string.IsNullOrWhiteSpace(session.CurrentMessage))
        {
            var text = session.CurrentMessage;
            var width = text.Length * 12 + 30;
            var rect = new Rectangle(viewport.Width / 2 - width / 2, 28, width, 36);
            ui.Panel(batch, rect, new Color(210, 180, 90), new Color(22, 22, 18, 225));
            ui.Label(batch, text, new Vector2(rect.X + 15, rect.Y + 10), Color.White, 2);
        }

        if (OperatingSystem.IsAndroid() || _game.Input.HasTouch) DrawTouchControls(batch, session, viewport);
        else ui.Label(batch, "WASD MOVE  SHIFT SPRINT  LEFT CLICK/SPACE ATTACK  RIGHT CLICK/E GATHER  B BUILD  Q EAT/DRINK  H HEAL  R SAVE", new Vector2(20, viewport.Height - 22), new Color(200, 205, 197), 1);
    }

    private void DrawTouchControls(SpriteBatch batch, WorldSession session, Rectangle viewport)
    {
        var ui = _game.Ui;
        var origin = TouchLayout.MoveOrigin(viewport);
        var movePad = TouchLayout.MovePad(viewport);
        ui.Panel(batch, new Rectangle((int)origin.X - 58, (int)origin.Y - 58, 116, 116), new Color(102, 106, 96) * 0.55f, new Color(20, 22, 22, 95));
        batch.Draw(_game.Art.Pixel, new Rectangle((int)origin.X - 8, (int)origin.Y - 8, 16, 16), new Color(220, 220, 200) * 0.60f);
        ui.Label(batch, "MOVE", new Vector2(movePad.X + 32, viewport.Height - 34), new Color(205, 210, 198), 1);

        DrawTouchButton(batch, TouchLayout.Attack(viewport), "ATK", new Color(204, 76, 66));
        DrawTouchButton(batch, TouchLayout.Gather(viewport), "GET", new Color(202, 160, 70));
        DrawTouchButton(batch, TouchLayout.Build(viewport), session.BuildMode ? "ON" : "BLD", new Color(110, 145, 210));
        DrawTouchButton(batch, TouchLayout.Eat(viewport), "EAT", new Color(205, 146, 68));
        DrawTouchButton(batch, TouchLayout.Heal(viewport), "MED", new Color(212, 218, 208));
        DrawTouchButton(batch, TouchLayout.Pause(viewport), "II", new Color(190, 190, 180));
    }

    private void DrawTouchButton(SpriteBatch batch, Rectangle rect, string label, Color border)
    {
        _game.Ui.Panel(batch, rect, border * 0.85f, new Color(13, 15, 15, 155));
        var scale = rect.Height >= 72 ? 2 : 1;
        var size = _game.Font.Measure(label, scale);
        _game.Font.DrawShadow(batch, label, new Vector2(rect.Center.X - size.X * 0.5f, rect.Center.Y - size.Y * 0.5f), Color.White, scale);
    }

    private void DrawItem(SpriteBatch batch, string label, int count, int x, int y)
    {
        _game.Ui.Label(batch, label, new Vector2(x, y), new Color(207, 211, 201), 1);
        _game.Ui.Label(batch, count.ToString(), new Vector2(x, y + 22), Color.White, 2);
    }

    private static string CostText(BuildableDefinition def)
    {
        return string.Join(" ", def.Cost.Select(c => $"{c.Key}:{c.Value}"));
    }

    private static string ClockText(WorldSession session)
    {
        var normalized = session.State.WorldTimeSeconds / Balance.DayLengthSeconds;
        var minutes = (int)(normalized * 24f * 60f);
        var hour = (minutes / 60) % 24;
        var minute = minutes % 60;
        return $"{hour:00}:{minute:00}";
    }
}
