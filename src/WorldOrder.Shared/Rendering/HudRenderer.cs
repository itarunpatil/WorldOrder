using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WorldOrder.Core;
using WorldOrder.Gameplay;

namespace WorldOrder.Rendering;

public sealed class HudRenderer
{
    private readonly GameRoot _game;

    public HudRenderer(GameRoot game) => _game = game;

    public void Draw(SpriteBatch batch, WorldSession session)
    {
        var viewport = _game.GraphicsDevice.Viewport.Bounds;
        DrawSurvivalPanel(batch, session, viewport);
        DrawQuickbar(batch, session, viewport);

        if (session.BuildMode) DrawBuildPanel(batch, session, viewport);
        if (!string.IsNullOrWhiteSpace(session.CurrentMessage)) DrawMessage(batch, session.CurrentMessage, viewport);
        if (session.State.Vitals.Health <= 0f) DrawDeathOverlay(batch, viewport);
        if (session.CraftingOpen) DrawCraftingOverlay(batch, session, viewport);

        if (OperatingSystem.IsAndroid() || _game.Input.HasTouch) DrawTouchControls(batch, session, viewport);
    }

    private void DrawSurvivalPanel(SpriteBatch batch, WorldSession session, Rectangle viewport)
    {
        var ui = _game.Ui;
        var v = session.State.Vitals;
        var panel = new Rectangle(18, 18, 326, 136);
        ui.Panel(batch, panel, new Color(83, 88, 80), new Color(15, 18, 17, 218));
        ui.Label(batch, $"DAY {session.State.Day}  {ClockText(session)}", new Vector2(panel.X + 16, panel.Y + 14), new Color(236, 226, 174), 2);
        DrawStatusBar(batch, panel.X + 16, panel.Y + 48, "HP", v.Health / 100f, new Color(188, 46, 44), new Color(47, 22, 22));
        DrawStatusBar(batch, panel.X + 16, panel.Y + 72, "FOOD", v.Hunger / 100f, new Color(207, 143, 62), new Color(52, 34, 22));
        DrawStatusBar(batch, panel.X + 16, panel.Y + 96, "WATER", v.Thirst / 100f, new Color(68, 139, 207), new Color(21, 39, 56));
        DrawStatusBar(batch, panel.X + 16, panel.Y + 120, "STAM", v.Stamina / 100f, new Color(75, 180, 92), new Color(22, 48, 27), 11);
    }

    private void DrawStatusBar(SpriteBatch batch, int x, int y, string label, float value, Color fill, Color back, int height = 14)
    {
        _game.Ui.Bar(batch, new Rectangle(x, y, 160, height), value, fill, back);
        _game.Ui.Label(batch, label, new Vector2(x + 174, y - 1), Color.White, 2);
    }

    private void DrawQuickbar(SpriteBatch batch, WorldSession session, Rectangle viewport)
    {
        var visible = session.State.Inventory.HotbarItems();
        if (session.SelectedHotbarIndex >= visible.Count) session.SelectedHotbarIndex = Math.Max(0, visible.Count - 1);
        var bar = HotbarPanel(viewport);
        _game.Ui.Panel(batch, bar, new Color(84, 88, 81), new Color(15, 18, 17, 208));

        for (var i = 0; i < Inventory.HotbarCapacity; i++)
        {
            var rect = HotbarSlotRect(viewport, i);
            if (i < visible.Count) DrawInventorySlot(batch, rect, visible[i].Item, visible[i].Count, i == session.SelectedHotbarIndex, i + 1);
            else DrawEmptySlot(batch, rect, i == session.SelectedHotbarIndex, i + 1);
        }
    }

    public static Rectangle HotbarPanel(Rectangle viewport)
    {
        var slotSize = Math.Clamp(viewport.Height / 11, 48, 68);
        var gap = 8;
        var count = Inventory.HotbarCapacity;
        var width = count * slotSize + (count - 1) * gap + 28;
        var x = viewport.Width / 2 - width / 2;
        var y = viewport.Height - slotSize - 30;
        return new Rectangle(x, y, width, slotSize + 20);
    }

    public static Rectangle HotbarSlotRect(Rectangle viewport, int index)
    {
        var panel = HotbarPanel(viewport);
        var slotSize = Math.Clamp(viewport.Height / 11, 48, 68);
        var gap = 8;
        return new Rectangle(panel.X + 14 + index * (slotSize + gap), panel.Y + 10, slotSize, slotSize);
    }

    private void DrawInventorySlot(SpriteBatch batch, Rectangle rect, ItemId item, int count, bool selected, int number)
    {
        batch.Draw(_game.Art.Texture(selected ? "ui_slot_selected" : "ui_slot"), rect, Color.White);
        var icon = _game.Art.Texture(ItemTextureKey(item));
        var iconSize = Math.Max(18, rect.Width - 18);
        var iconRect = new Rectangle(rect.Center.X - iconSize / 2, rect.Y + 8, iconSize, iconSize);
        batch.Draw(icon, iconRect, count > 0 ? Color.White : Color.White * 0.28f);
        _game.Font.DrawShadow(batch, number.ToString(), new Vector2(rect.X + 5, rect.Y + 4), new Color(210, 214, 200), 1);
        if (count > 0) _game.Font.DrawShadow(batch, count.ToString(), new Vector2(rect.Right - 18, rect.Bottom - 18), Color.White, 1);
    }

    private void DrawEmptySlot(SpriteBatch batch, Rectangle rect, bool selected, int number)
    {
        batch.Draw(_game.Art.Texture(selected ? "ui_slot_selected" : "ui_slot"), rect, Color.White * (selected ? 1f : 0.72f));
        _game.Font.DrawShadow(batch, number.ToString(), new Vector2(rect.X + 5, rect.Y + 4), new Color(130, 135, 126), 1);
    }

    private void DrawBuildPanel(SpriteBatch batch, WorldSession session, Rectangle viewport)
    {
        var def = GameDefinitions.Buildables[MathTools.ClampInt(session.SelectedBuildableIndex, 0, GameDefinitions.Buildables.Length - 1)];
        var rect = new Rectangle(viewport.Width - 374, 76, 346, 96);
        _game.Ui.Panel(batch, rect, new Color(211, 170, 80), new Color(22, 24, 23, 225));
        _game.Ui.Label(batch, $"BUILD: {def.Name}".ToUpperInvariant(), new Vector2(rect.X + 18, rect.Y + 16), new Color(244, 229, 154), 2);
        _game.Ui.Label(batch, "1-4 SELECT  TAB NEXT", new Vector2(rect.X + 18, rect.Y + 44), Color.White, 2);
        _game.Ui.Label(batch, CostText(def.Cost), new Vector2(rect.X + 18, rect.Y + 70), new Color(190, 212, 184), 2);
    }

    private void DrawDeathOverlay(SpriteBatch batch, Rectangle viewport)
    {
        var rect = new Rectangle(viewport.Width / 2 - 270, 58, 540, 54);
        _game.Ui.Panel(batch, rect, new Color(220, 68, 58), new Color(19, 18, 16, 232));
        _game.Ui.Label(batch, "YOU DIED - LOAD OR START AGAIN", new Vector2(rect.X + 26, rect.Y + 16), new Color(245, 230, 190), 2);
    }

    private void DrawMessage(SpriteBatch batch, string text, Rectangle viewport)
    {
        var width = text.Length * 12 + 30;
        var rect = new Rectangle(viewport.Width / 2 - width / 2, 28, width, 36);
        _game.Ui.Panel(batch, rect, new Color(210, 180, 90), new Color(22, 22, 18, 225));
        _game.Ui.Label(batch, text, new Vector2(rect.X + 15, rect.Y + 10), Color.White, 2);
    }

    public static Rectangle CraftingPanel(Rectangle viewport)
    {
        var width = Math.Min(760, viewport.Width - 96);
        var height = Math.Min(540, viewport.Height - 120);
        return new Rectangle(viewport.Width / 2 - width / 2, 72, width, height);
    }

    public static Rectangle CraftingRecipeRect(Rectangle viewport, int index)
    {
        var panel = CraftingPanel(viewport);
        var width = Math.Max(340, panel.Width - 330);
        return new Rectangle(panel.X + 34, panel.Y + 104 + index * 70, width, 56);
    }

    private void DrawCraftingOverlay(SpriteBatch batch, WorldSession session, Rectangle viewport)
    {
        batch.Draw(_game.Art.Pixel, viewport, Color.Black * 0.42f);
        var panel = CraftingPanel(viewport);
        _game.Ui.Panel(batch, panel, new Color(214, 176, 88), new Color(18, 20, 19, 238));
        _game.Ui.Label(batch, "CRAFTING & INVENTORY", new Vector2(panel.X + 34, panel.Y + 30), new Color(238, 224, 156), 4);
        _game.Ui.Label(batch, "AVAILABLE RECIPES", new Vector2(panel.X + 36, panel.Y + 72), new Color(196, 205, 188), 1);

        var inventory = session.State.Inventory;
        for (var i = 0; i < GameDefinitions.Recipes.Length; i++)
        {
            var recipe = GameDefinitions.Recipes[i];
            var rect = CraftingRecipeRect(viewport, i);
            var canCraft = inventory.HasCost(recipe.Cost);
            _game.Ui.Panel(batch, rect, canCraft ? new Color(142, 170, 101) : new Color(88, 86, 80), canCraft ? new Color(35, 45, 31, 225) : new Color(28, 29, 28, 225));
            var iconRect = new Rectangle(rect.X + 12, rect.Y + 8, 40, 40);
            batch.Draw(_game.Art.Texture(ItemTextureKey(recipe.Result)), iconRect, canCraft ? Color.White : Color.White * 0.36f);
            _game.Ui.Label(batch, $"{i + 1}. {recipe.Name.ToUpperInvariant()}  x{recipe.Count}", new Vector2(rect.X + 66, rect.Y + 9), canCraft ? Color.White : new Color(150, 156, 146), 2);
            _game.Ui.Label(batch, CostText(recipe.Cost), new Vector2(rect.X + 66, rect.Y + 35), canCraft ? new Color(211, 226, 188) : new Color(140, 145, 135), 1);
        }

        DrawInventoryList(batch, inventory, panel);
    }

    private void DrawInventoryList(SpriteBatch batch, Inventory inventory, Rectangle panel)
    {
        var area = new Rectangle(panel.Right - 270, panel.Y + 104, 236, panel.Height - 136);
        _game.Ui.Panel(batch, area, new Color(86, 91, 84), new Color(15, 17, 17, 220));
        _game.Ui.Label(batch, "BAG", new Vector2(area.X + 16, area.Y + 14), new Color(236, 220, 150), 3);
        var stacks = inventory.Items.Where(i => i.Count > 0).OrderBy(i => Array.IndexOf(Inventory.HotbarOrder, i.Item) < 0 ? 999 : Array.IndexOf(Inventory.HotbarOrder, i.Item)).ToList();
        if (stacks.Count == 0)
        {
            _game.Ui.Label(batch, "EMPTY", new Vector2(area.X + 18, area.Y + 64), new Color(150, 155, 146), 2);
            return;
        }
        for (var i = 0; i < stacks.Count; i++)
        {
            var stack = stacks[i];
            var y = area.Y + 58 + i * 38;
            var iconRect = new Rectangle(area.X + 16, y, 28, 28);
            batch.Draw(_game.Art.Texture(ItemTextureKey(stack.Item)), iconRect, Color.White);
            _game.Ui.Label(batch, $"{GameDefinitions.ItemName(stack.Item).ToUpperInvariant()}  x{stack.Count}", new Vector2(area.X + 54, y + 6), Color.White, 1);
        }
    }

    private void DrawTouchControls(SpriteBatch batch, WorldSession session, Rectangle viewport)
    {
        var ui = _game.Ui;
        if (!session.CraftingOpen)
        {
            var origin = TouchLayout.MoveOrigin(viewport);
            var movePad = TouchLayout.MovePad(viewport);
            ui.Panel(batch, new Rectangle((int)origin.X - 58, (int)origin.Y - 58, 116, 116), new Color(102, 106, 96) * 0.55f, new Color(20, 22, 22, 95));
            batch.Draw(_game.Art.Pixel, new Rectangle((int)origin.X - 8, (int)origin.Y - 8, 16, 16), new Color(220, 220, 200) * 0.60f);
            DrawTouchButton(batch, TouchLayout.Attack(viewport), "ATK", new Color(204, 76, 66));
            DrawTouchButton(batch, TouchLayout.Gather(viewport), "GET", new Color(202, 160, 70));
            DrawTouchButton(batch, TouchLayout.Build(viewport), session.BuildMode ? "ON" : "BLD", new Color(110, 145, 210));
            DrawTouchButton(batch, TouchLayout.Eat(viewport), "EAT", new Color(205, 146, 68));
            DrawTouchButton(batch, TouchLayout.Heal(viewport), "MED", new Color(212, 218, 208));
        }
        DrawTouchButton(batch, TouchLayout.Inventory(viewport), session.CraftingOpen ? "X" : "INV", new Color(196, 175, 92));
        DrawTouchButton(batch, TouchLayout.Pause(viewport), session.CraftingOpen ? "X" : "II", new Color(190, 190, 180));
    }

    private void DrawTouchButton(SpriteBatch batch, Rectangle rect, string label, Color border)
    {
        _game.Ui.Panel(batch, rect, border * 0.85f, new Color(13, 15, 15, 155));
        var scale = rect.Height >= 72 ? 2 : 1;
        var size = _game.Font.Measure(label, scale);
        _game.Font.DrawShadow(batch, label, new Vector2(rect.Center.X - size.X * 0.5f, rect.Center.Y - size.Y * 0.5f), Color.White, scale);
    }

    public static string ItemTextureKey(ItemId item) => item switch
    {
        ItemId.Wood => "wood",
        ItemId.Scrap => "icon_scrap",
        ItemId.Stone => "icon_rock",
        ItemId.Cloth => "icon_cloth",
        ItemId.Food => "icon_food",
        ItemId.Water => "icon_water",
        ItemId.Bandage => "icon_bandage",
        ItemId.Ammo => "icon_ammo",
        ItemId.Pistol => "icon_pistol",
        _ => "crate"
    };

    private static string CostText(IReadOnlyDictionary<ItemId, int> cost)
    {
        return string.Join("  ", cost.Select(c => $"{GameDefinitions.ItemName(c.Key).ToUpperInvariant()}:{c.Value}"));
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
