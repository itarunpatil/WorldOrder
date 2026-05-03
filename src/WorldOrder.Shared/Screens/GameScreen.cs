using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace WorldOrder;

public sealed class GameScreen : IGameScreen
{
    private readonly Game1 _game;
    private readonly RtsSession _session;
    private Vector2 _camera;
    private float _zoom = 0.72f;
    private bool _draggingSelection;
    private Vector2 _selectStart;
    private Vector2 _selectEnd;
    private readonly List<Unit> _selected = new();
    private float _hintTimer = 5f;
    private string _toast = "Select units and destroy enemy command centers";
    private readonly List<Rectangle> _uiBlockers = new();
    private InputState _lastInput = new();

    public GameScreen(Game1 game, RtsSession session)
    {
        _game = game;
        _session = session;
        _camera = session.Map.PlayerSpawn;
    }

    public void Update(GameTime time, InputState input)
    {
        _lastInput = input;
        float dt = MathEx.Dt(time);
        _hintTimer = MathEx.Approach(_hintTimer, 0f, dt);
        UpdateUiBlockers();
        HandleCamera(input, dt);
        HandleCommands(input);
        _session.Update(time);
        _selected.RemoveAll(u => !u.Alive || !_session.Units.Contains(u));
        foreach (var u in _session.Units) u.Selected = _selected.Contains(u);
        if (_session.Victory) _toast = "Victory. Enemy command network destroyed.";
        if (_session.Defeat) _toast = "Defeat. Your command center was eliminated.";
    }

    private void UpdateUiBlockers()
    {
        _uiBlockers.Clear();
        var vp = _game.GraphicsDevice.Viewport;
        _uiBlockers.Add(TopBar(vp));
        _uiBlockers.Add(BottomBar(vp));
        _uiBlockers.Add(MiniMapRect(vp));
    }

    private bool PointerOverUi(InputState input) => _uiBlockers.Any(r => r.Contains(input.Pointer));

    private static Rectangle TopBar(Viewport vp) => new(0, 0, vp.Width, 72);
    private static int BottomBarHeight(Viewport vp) => vp.Height < 650 ? 104 : 116;
    private static Rectangle BottomBar(Viewport vp) => new(0, vp.Height - BottomBarHeight(vp), vp.Width, BottomBarHeight(vp));

    private static Rectangle MiniMapRect(Viewport vp)
    {
        int width = Math.Clamp(vp.Width / 7, 150, 220);
        int height = Math.Clamp(vp.Height / 5, 116, 164);
        return new Rectangle(vp.Width - width - 18, 86, width, height);
    }

    private static Rectangle LightButton(Viewport vp)
    {
        var bar = BottomBar(vp);
        int w = Math.Clamp(vp.Width / 9, 112, 156);
        return new Rectangle(22, bar.Y + 20, w, 54);
    }

    private static Rectangle HeavyButton(Viewport vp)
    {
        var first = LightButton(vp);
        int w = Math.Clamp(first.Width + 4, 116, 162);
        return new Rectangle(first.Right + 12, first.Y, w, first.Height);
    }

    private static Rectangle HarvesterButton(Viewport vp)
    {
        var second = HeavyButton(vp);
        int w = Math.Clamp(second.Width + 4, 118, 166);
        return new Rectangle(second.Right + 12, second.Y, w, second.Height);
    }

    private static Rectangle CenterButton(Viewport vp)
    {
        var third = HarvesterButton(vp);
        int w = Math.Clamp(third.Width + 4, 120, 168);
        return new Rectangle(third.Right + 12, third.Y, w, third.Height);
    }

    private static Rectangle BackButton(Viewport vp)
    {
        var bar = BottomBar(vp);
        int w = Math.Clamp(vp.Width / 10, 112, 144);
        return new Rectangle(vp.Width - w - 22, bar.Y + 20, w, 54);
    }

    private void HandleCamera(InputState input, float dt)
    {
        var vp = _game.GraphicsDevice.Viewport;
        Vector2 move = Vector2.Zero;
        if (input.KeyDown(Keys.A) || input.KeyDown(Keys.Left)) move.X -= 1;
        if (input.KeyDown(Keys.D) || input.KeyDown(Keys.Right)) move.X += 1;
        if (input.KeyDown(Keys.W) || input.KeyDown(Keys.Up)) move.Y -= 1;
        if (input.KeyDown(Keys.S) || input.KeyDown(Keys.Down)) move.Y += 1;

        if (input.PointerDown && !PointerOverUi(input))
        {
            int edge = 24;
            if (input.Pointer.X < edge) move.X -= 1;
            if (input.Pointer.X > vp.Width - edge) move.X += 1;
            if (input.Pointer.Y < edge) move.Y -= 1;
            if (input.Pointer.Y > vp.Height - edge) move.Y += 1;
        }

        if (move.LengthSquared() > 0f)
        {
            move.Normalize();
            _camera += move * (620f / _zoom) * dt;
        }

        if (Math.Abs(input.ScrollDelta) > 0.01f)
        {
            _zoom = MathHelper.Clamp(_zoom + Math.Sign(input.ScrollDelta) * 0.08f, 0.45f, 1.45f);
        }

        ClampCamera();
    }

    private void ClampCamera()
    {
        var vp = _game.GraphicsDevice.Viewport;
        float halfW = vp.Width / (2f * _zoom);
        float halfH = vp.Height / (2f * _zoom);
        float minX = Math.Min(halfW, _session.Map.PixelWidth / 2f);
        float maxX = Math.Max(minX, _session.Map.PixelWidth - halfW);
        float minY = Math.Min(halfH, _session.Map.PixelHeight / 2f);
        float maxY = Math.Max(minY, _session.Map.PixelHeight - halfH);
        _camera.X = MathHelper.Clamp(_camera.X, minX, maxX);
        _camera.Y = MathHelper.Clamp(_camera.Y, minY, maxY);
    }

    private void HandleCommands(InputState input)
    {
        if (_session.Victory || _session.Defeat)
        {
            if (input.PointerReleased || input.KeyPressed(Keys.Enter)) _game.Screens.Change(new WorldSelectScreen(_game));
            return;
        }

        var vp = _game.GraphicsDevice.Viewport;
        var lightButton = LightButton(vp);
        var heavyButton = HeavyButton(vp);
        var harvesterButton = HarvesterButton(vp);
        var centerButton = CenterButton(vp);
        var backButton = BackButton(vp);
        if (lightButton.Contains(input.Pointer) && input.PointerReleased)
        {
            if (!_session.TryBuildTank(UnitKind.LightTank)) ShowToast("Need 170 supplies and an active command center");
            else ShowToast("Light tank queued at command center");
            return;
        }
        if (heavyButton.Contains(input.Pointer) && input.PointerReleased)
        {
            if (!_session.TryBuildTank(UnitKind.HeavyTank)) ShowToast("Need 280 supplies and an active command center");
            else ShowToast("Heavy tank rolled out");
            return;
        }
        if (harvesterButton.Contains(input.Pointer) && input.PointerReleased)
        {
            if (!_session.TryBuildTank(UnitKind.Harvester)) ShowToast("Need 140 supplies and an active command center");
            else ShowToast("Harvester deployed. It will mine spice automatically.");
            return;
        }
        if (centerButton.Contains(input.Pointer) && input.PointerReleased)
        {
            _camera = _session.Map.PlayerSpawn;
            ClampCamera();
            return;
        }
        if (backButton.Contains(input.Pointer) && input.PointerReleased)
        {
            _game.Screens.Change(new WorldSelectScreen(_game));
            return;
        }

        if (PointerOverUi(input)) return;
        var world = ScreenToWorld(input.Pointer);

        if (input.SecondaryReleased)
        {
            IssueCommand(world);
            return;
        }

        if (input.PointerPressed)
        {
            _draggingSelection = true;
            _selectStart = input.Pointer;
            _selectEnd = input.Pointer;
        }
        if (_draggingSelection && input.PointerDown)
        {
            _selectEnd = input.Pointer;
        }
        if (_draggingSelection && input.PointerReleased)
        {
            _draggingSelection = false;
            _selectEnd = input.Pointer;
            var rect = MathEx.RectFromPoints(_selectStart, _selectEnd);
            if (rect.Width < 10 && rect.Height < 10)
            {
                var unit = UnitAt(world);
                if (unit != null)
                {
                    if (unit.Faction == FactionKind.Player)
                    {
                        _selected.Clear();
                        _selected.Add(unit);
                        ShowToast(unit.Kind + " selected");
                    }
                    else if (_selected.Count > 0 && unit.Faction == FactionKind.Enemy)
                    {
                        _session.CommandAttack(_selected, unit);
                        ShowToast("Attack order confirmed");
                    }
                }
                else if (input.HasTouch && _selected.Count > 0)
                {
                    _session.CommandMove(_selected, world);
                    ShowToast("Move order confirmed");
                }
            }
            else
            {
                SelectUnits(rect);
            }
        }
    }

    private void IssueCommand(Vector2 world)
    {
        if (_selected.Count == 0) return;
        var target = UnitAt(world);
        if (target != null && target.Faction == FactionKind.Enemy)
        {
            _session.CommandAttack(_selected, target);
            ShowToast("Attack order confirmed");
        }
        else
        {
            _session.CommandMove(_selected, world);
            ShowToast("Move order confirmed");
        }
    }

    private void SelectUnits(Rectangle screenRect)
    {
        _selected.Clear();
        foreach (var unit in _session.Units.Where(u => u.Alive && u.Faction == FactionKind.Player))
        {
            var pos = WorldToScreen(unit.Position);
            if (screenRect.Contains(pos)) _selected.Add(unit);
        }
        ShowToast(_selected.Count == 0 ? "No units selected" : $"{_selected.Count} units selected");
    }

    private Unit? UnitAt(Vector2 world)
    {
        return _session.Units
            .Where(u => u.Alive && (u.Faction != FactionKind.Enemy || _session.IsVisibleWorld(u.Position)))
            .OrderBy(u => Vector2.DistanceSquared(u.Position, world))
            .FirstOrDefault(u => Vector2.DistanceSquared(u.Position, world) <= (u.Radius + 16f) * (u.Radius + 16f));
    }

    private void ShowToast(string message)
    {
        _toast = message;
        _hintTimer = 3.2f;
    }

    private Matrix CameraMatrix()
    {
        var vp = _game.GraphicsDevice.Viewport;
        return Matrix.CreateTranslation(new Vector3(-_camera, 0)) * Matrix.CreateScale(_zoom) * Matrix.CreateTranslation(new Vector3(vp.Width / 2f, vp.Height / 2f, 0));
    }

    private Vector2 ScreenToWorld(Vector2 screen)
    {
        return Vector2.Transform(screen, Matrix.Invert(CameraMatrix()));
    }

    private Vector2 WorldToScreen(Vector2 world)
    {
        return Vector2.Transform(world, CameraMatrix());
    }

    public void Draw(GameTime time, SpriteBatch batch)
    {
        DrawWorld(time, batch);
        DrawUi(time, batch);
    }

    private void DrawWorld(GameTime time, SpriteBatch batch)
    {
        var vp = _game.GraphicsDevice.Viewport;
        var matrix = CameraMatrix();
        batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, matrix);
        DrawMap(batch);
        DrawResources(batch);
        DrawProps(batch);
        DrawProjectiles(batch);
        DrawUnits(time, batch);
        DrawParticles(batch);
        DrawFog(batch);
        batch.End();

        if (_draggingSelection)
        {
            batch.Begin(samplerState: SamplerState.LinearClamp);
            var r = MathEx.RectFromPoints(_selectStart, _selectEnd);
            UiKit.Fill(batch, _game.Pixel, r, new Color(80, 180, 115, 35));
            UiKit.Outline(batch, _game.Pixel, r, new Color(125, 225, 145, 220), 2);
            batch.End();
        }
    }

    private void DrawMap(SpriteBatch batch)
    {
        var vp = _game.GraphicsDevice.Viewport;
        var tl = ScreenToWorld(Vector2.Zero);
        var br = ScreenToWorld(new Vector2(vp.Width, vp.Height));
        int tx0 = Math.Max(0, (int)(tl.X / _session.Map.TileSize) - 2);
        int ty0 = Math.Max(0, (int)(tl.Y / _session.Map.TileSize) - 2);
        int tx1 = Math.Min(_session.Map.Width - 1, (int)(br.X / _session.Map.TileSize) + 2);
        int ty1 = Math.Min(_session.Map.Height - 1, (int)(br.Y / _session.Map.TileSize) + 2);
        for (int y = ty0; y <= ty1; y++)
        {
            for (int x = tx0; x <= tx1; x++)
            {
                string key = _session.Map.TextureKeys[x, y];
                var texture = _game.Assets.Get(key);
                var dest = new Rectangle(x * _session.Map.TileSize, y * _session.Map.TileSize, _session.Map.TileSize, _session.Map.TileSize);
                var tint = _session.Map.Tiles[x, y] switch
                {
                    TileKind.Water => new Color(126, 164, 179),
                    TileKind.Rock => new Color(145, 116, 77),
                    TileKind.Road => new Color(165, 133, 89),
                    _ => Color.White
                };
                batch.Draw(texture, dest, tint);
            }
        }
    }

    private void DrawResources(SpriteBatch batch)
    {
        var tex = _game.Assets.Get("terrain_resource_spice");
        foreach (var node in _session.Map.ResourceNodes)
        {
            if (!_session.IsExploredWorld(node.Position)) continue;
            float fullness = node.MaxAmount <= 0 ? 0f : MathHelper.Clamp(node.Amount / (float)node.MaxAmount, 0f, 1f);
            if (fullness <= 0.02f) continue;
            var tint = _session.IsVisibleWorld(node.Position) ? Color.White : Color.White * 0.42f;
            batch.Draw(tex, node.Position, null, tint * MathHelper.Lerp(0.45f, 0.95f, fullness), 0f, new Vector2(tex.Width / 2f, tex.Height / 2f), node.Radius / 72f, SpriteEffects.None, 0f);
        }
    }

    private void DrawProps(SpriteBatch batch)
    {
        foreach (var prop in _session.Map.Props)
        {
            var tex = _game.Assets.Get(prop.TextureKey);
            batch.Draw(tex, prop.Position, null, Color.White * 0.9f, prop.Rotation, new Vector2(tex.Width / 2f, tex.Height / 2f), prop.Scale, SpriteEffects.None, 0f);
        }
    }

    private void DrawUnits(GameTime time, SpriteBatch batch)
    {
        float t = (float)time.TotalGameTime.TotalSeconds;
        foreach (var u in _session.Units.OrderBy(u => u.Position.Y))
        {
            if (!u.Alive) continue;
            if ((u.Faction == FactionKind.Enemy || u.Faction == FactionKind.Neutral) && !_session.IsVisibleWorld(u.Position)) continue;
            var shadow = new Rectangle((int)(u.Position.X - u.Radius), (int)(u.Position.Y + u.Radius * 0.45f), (int)(u.Radius * 2), (int)(u.Radius * 0.55f));
            UiKit.Fill(batch, _game.Pixel, shadow, new Color(0, 0, 0, 64));
            if (u.Selected)
            {
                var ring = _game.Assets.Get("selection_ring");
                batch.Draw(ring, u.Position, null, Color.White, 0f, new Vector2(64, 64), u.Radius / 44f, SpriteEffects.None, 0f);
            }

            if (u.Naval)
            {
                int frame = 1 + ((int)(t * 7f + u.Id) % 4);
                int color = u.Faction == FactionKind.Enemy ? 2 : u.Faction == FactionKind.Ally ? 3 : 1;
                var boat = _game.Assets.Get($"boat_water_1_{color}_{frame}");
                batch.Draw(boat, u.Position, null, Color.White, u.Rotation, new Vector2(64, 64), 0.72f, SpriteEffects.None, 0f);
            }
            else
            {
                var hull = _game.Assets.Get(u.HullKey);
                var gun = _game.Assets.Get(u.GunKey);
                float scale = u.Structure ? 0.58f : u.Kind == UnitKind.HeavyTank ? 0.43f : 0.38f;
                batch.Draw(hull, u.Position, null, Color.White, u.Rotation, new Vector2(hull.Width / 2f, hull.Height / 2f), scale, SpriteEffects.None, 0f);
                batch.Draw(gun, u.Position, null, Color.White, u.TurretRotation, new Vector2(gun.Width / 2f, gun.Height * 0.62f), scale, SpriteEffects.None, 0f);
            }
            DrawHealth(batch, u);
        }
    }

    private void DrawHealth(SpriteBatch batch, Unit u)
    {
        int w = (int)(u.Radius * 1.65f);
        var bg = new Rectangle((int)(u.Position.X - w / 2), (int)(u.Position.Y - u.Radius - 20), w, 8);
        UiKit.Fill(batch, _game.Pixel, bg, new Color(25, 20, 18, 200));
        var fg = new Rectangle(bg.X + 1, bg.Y + 1, (int)((w - 2) * MathEx.Clamp01(u.Health / u.MaxHealth)), 6);
        Color c = u.Faction == FactionKind.Enemy ? UiKit.Red : u.Faction == FactionKind.Ally ? new Color(79, 198, 202) : UiKit.Green;
        UiKit.Fill(batch, _game.Pixel, fg, c);
        if (u.Worker && u.CargoCapacity > 0)
        {
            var cargoBg = new Rectangle(bg.X, bg.Bottom + 2, bg.Width, 5);
            UiKit.Fill(batch, _game.Pixel, cargoBg, new Color(30, 23, 16, 200));
            var cargo = new Rectangle(cargoBg.X + 1, cargoBg.Y + 1, (int)((cargoBg.Width - 2) * MathEx.Clamp01(u.Cargo / (float)u.CargoCapacity)), 3);
            UiKit.Fill(batch, _game.Pixel, cargo, new Color(238, 173, 59));
        }
    }

    private void DrawProjectiles(SpriteBatch batch)
    {
        foreach (var p in _session.Projectiles)
        {
            var angle = MathEx.RotationToVelocity(p.Velocity, 0f);
            var rect = new Rectangle((int)p.Position.X, (int)p.Position.Y, 8, 26);
            batch.Draw(_game.Pixel, rect, null, new Color(249, 214, 116), angle, new Vector2(4, 13), SpriteEffects.None, 0f);
        }
    }

    private void DrawParticles(SpriteBatch batch)
    {
        foreach (var p in _session.Particles)
        {
            int frame;
            Texture2D tex;
            if (p.Prefix == "flash_")
            {
                frame = p.Age > p.Duration * 0.5f ? 2 : 1;
                tex = _game.Assets.Get("flash_" + frame);
            }
            else
            {
                frame = Math.Clamp((int)(p.Age / p.Duration * 9), 0, 8);
                tex = _game.Assets.Get("explosion_" + frame);
            }
            batch.Draw(tex, p.Position, null, Color.White, 0f, new Vector2(tex.Width / 2f, tex.Height / 2f), p.Scale, SpriteEffects.None, 0f);
        }
    }

    private void DrawFog(SpriteBatch batch)
    {
        var vp = _game.GraphicsDevice.Viewport;
        var tl = ScreenToWorld(Vector2.Zero);
        var br = ScreenToWorld(new Vector2(vp.Width, vp.Height));
        int tx0 = Math.Max(0, (int)(tl.X / _session.Map.TileSize) - 2);
        int ty0 = Math.Max(0, (int)(tl.Y / _session.Map.TileSize) - 2);
        int tx1 = Math.Min(_session.Map.Width - 1, (int)(br.X / _session.Map.TileSize) + 2);
        int ty1 = Math.Min(_session.Map.Height - 1, (int)(br.Y / _session.Map.TileSize) + 2);
        for (int y = ty0; y <= ty1; y++)
        for (int x = tx0; x <= tx1; x++)
        {
            if (_session.VisibleTiles[x, y]) continue;
            int alpha = _session.ExploredTiles[x, y] ? 108 : 218;
            UiKit.Fill(batch, _game.Pixel, new Rectangle(x * _session.Map.TileSize, y * _session.Map.TileSize, _session.Map.TileSize, _session.Map.TileSize), new Color(0, 0, 0, alpha));
        }
    }

    private void DrawUi(GameTime time, SpriteBatch batch)
    {
        var vp = _game.GraphicsDevice.Viewport;
        var top = TopBar(vp);
        var bottom = BottomBar(vp);
        batch.Begin(samplerState: SamplerState.LinearClamp);

        UiKit.Fill(batch, _game.Pixel, top, new Color(31, 25, 20, 246));
        UiKit.Fill(batch, _game.Pixel, new Rectangle(0, top.Bottom - 4, vp.Width, 4), UiKit.Accent);
        UiKit.DrawFitted(_game, batch, "WORLD ORDER", new Vector2(22, 18), UiKit.Ink, 0.78f, 210);
        UiKit.DrawFitted(_game, batch, $"SUPPLIES {_session.Supplies}", new Vector2(238, 18), UiKit.Ink, 0.67f, 190);
        UiKit.DrawFitted(_game, batch, $"UNITS {_session.Units.Count(u => u.Faction == FactionKind.Player)}", new Vector2(426, 18), UiKit.InkDim, 0.58f, 140);
        UiKit.DrawFitted(_game, batch, _session.Objective, new Vector2(590, 18), UiKit.InkDim, 0.54f, Math.Max(220, vp.Width - 860));
        DrawMiniMap(batch);

        UiKit.Fill(batch, _game.Pixel, bottom, new Color(31, 25, 20, 246));
        UiKit.Fill(batch, _game.Pixel, new Rectangle(0, bottom.Y, vp.Width, 4), UiKit.Accent);
        var lightButton = LightButton(vp);
        var heavyButton = HeavyButton(vp);
        var harvesterButton = HarvesterButton(vp);
        var centerButton = CenterButton(vp);
        var backButton = BackButton(vp);
        UiKit.Button(_game, batch, _lastInput, lightButton, "LIGHT 170", true, 0.50f);
        UiKit.Button(_game, batch, _lastInput, heavyButton, "HEAVY 280", true, 0.50f);
        UiKit.Button(_game, batch, _lastInput, harvesterButton, "MINE 140", true, 0.50f);
        UiKit.Button(_game, batch, _lastInput, centerButton, "CENTER", true, 0.50f);
        UiKit.Button(_game, batch, _lastInput, backButton, "EXIT", true, 0.58f);

        int textX = centerButton.Right + 22;
        int textRight = backButton.X - 20;
        int textWidth = Math.Max(160, textRight - textX);
        if (textWidth > 180)
        {
            UiKit.DrawFitted(_game, batch, _selected.Count == 0 ? "No selection" : $"Selected: {_selected.Count} unit(s)", new Vector2(textX, bottom.Y + 24), UiKit.Ink, 0.58f, textWidth);
            UiKit.DrawFitted(_game, batch, _game.IsMobile ? "Tap unit, then tap ground or enemy. Harvesters mine automatically." : "Drag-select | Right-click move/attack | Wheel zoom | WASD pan | Harvesters mine automatically", new Vector2(textX, bottom.Y + 58), UiKit.InkDim, 0.43f, textWidth);
        }

        if (_hintTimer > 0f)
        {
            int width = Math.Min(720, vp.Width - 120);
            var r = new Rectangle(vp.Width / 2 - width / 2, top.Bottom + 16, width, 44);
            UiKit.Fill(batch, _game.Pixel, r, new Color(35, 29, 23, 224));
            UiKit.Outline(batch, _game.Pixel, r, new Color(166, 123, 67), 2);
            UiKit.DrawCenteredFitted(_game, batch, _toast, r, UiKit.Ink, 0.54f);
        }

        if (_session.Victory || _session.Defeat)
        {
            int ow = Math.Min(720, vp.Width - 120);
            var overlay = new Rectangle(vp.Width / 2 - ow / 2, vp.Height / 2 - 120, ow, 240);
            UiKit.PanelBox(_game, batch, overlay);
            UiKit.DrawCenteredFitted(_game, batch, _session.Victory ? "VICTORY" : "DEFEAT", new Rectangle(overlay.X, overlay.Y + 45, overlay.Width, 55), _session.Victory ? UiKit.Green : UiKit.Red, 1.15f);
            UiKit.DrawCenteredFitted(_game, batch, "Tap or press Enter to return to the world list.", new Rectangle(overlay.X + 24, overlay.Y + 125, overlay.Width - 48, 40), UiKit.InkDim, 0.58f);
        }
        batch.End();
    }

    private void DrawMiniMap(SpriteBatch batch)
    {
        var vp = _game.GraphicsDevice.Viewport;
        var r = MiniMapRect(vp);
        UiKit.Fill(batch, _game.Pixel, r, new Color(19, 17, 15, 230));
        UiKit.Outline(batch, _game.Pixel, r, new Color(115, 84, 47), 2);
        float sx = r.Width / (float)_session.Map.Width;
        float sy = r.Height / (float)_session.Map.Height;
        for (int y = 0; y < _session.Map.Height; y += 2)
        for (int x = 0; x < _session.Map.Width; x += 2)
        {
            Color c = _session.Map.Tiles[x, y] switch
            {
                TileKind.Water => new Color(67, 126, 150),
                TileKind.Rock => new Color(101, 76, 52),
                TileKind.Road => new Color(151, 119, 74),
                _ => new Color(187, 140, 75)
            };
            UiKit.Fill(batch, _game.Pixel, new Rectangle(r.X + (int)(x * sx), r.Y + (int)(y * sy), Math.Max(1, (int)(sx * 2)), Math.Max(1, (int)(sy * 2))), c);
        }
        foreach (var node in _session.Map.ResourceNodes)
        {
            if (!_session.IsExploredWorld(node.Position) || node.Depleted) continue;
            int x = r.X + (int)(node.Position.X / _session.Map.PixelWidth * r.Width);
            int y = r.Y + (int)(node.Position.Y / _session.Map.PixelHeight * r.Height);
            UiKit.Fill(batch, _game.Pixel, new Rectangle(x - 2, y - 2, 5, 5), new Color(238, 173, 59));
        }
        foreach (var u in _session.Units)
        {
            if (!u.Alive) continue;
            if (u.Faction == FactionKind.Enemy && !_session.IsVisibleWorld(u.Position)) continue;
            Color c = u.Faction == FactionKind.Enemy ? UiKit.Red : u.Faction == FactionKind.Ally ? new Color(78, 200, 212) : UiKit.Green;
            int x = r.X + (int)(u.Position.X / _session.Map.PixelWidth * r.Width);
            int y = r.Y + (int)(u.Position.Y / _session.Map.PixelHeight * r.Height);
            UiKit.Fill(batch, _game.Pixel, new Rectangle(x - 2, y - 2, 4, 4), c);
        }
        var cam = new Rectangle(r.X + (int)(_camera.X / _session.Map.PixelWidth * r.Width) - 5, r.Y + (int)(_camera.Y / _session.Map.PixelHeight * r.Height) - 4, 10, 8);
        UiKit.Outline(batch, _game.Pixel, cam, Color.White, 1);
    }
}
