using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WorldOrder;

public sealed class AssetBank : IDisposable
{
    private readonly GraphicsDevice _graphics;
    private readonly Dictionary<string, Texture2D> _textures = new(StringComparer.OrdinalIgnoreCase);
    public Texture2D Pixel { get; private set; } = null!;

    public AssetBank(GraphicsDevice graphics) => _graphics = graphics;

    public Texture2D Get(string key) => _textures.TryGetValue(key, out var value) ? value : Pixel;

    public void LoadCore()
    {
        Pixel = new Texture2D(_graphics, 1, 1);
        Pixel.SetData(new[] { Color.White });
        _textures["pixel"] = Pixel;
        Load("font_hud", "Content/Fonts/hud_font.png");
        Load("selection_ring", "Content/Assets/UI/selection_ring.png");
        Load("attack_reticle", "Content/Assets/UI/attack_reticle.png");

        foreach (var name in new[] { "01", "02", "03", "04", "05", "06", "07", "08" })
        {
            Load($"hull_player_{name}", $"Content/Assets/Tanks/hull_sand_{name}.png");
            Load($"hull_enemy_{name}", $"Content/Assets/Tanks/hull_olive_{name}.png");
            Load($"hull_ally_{name}", $"Content/Assets/Tanks/hull_teal_{name}.png");
            Load($"hull_neutral_{name}", $"Content/Assets/Tanks/hull_blue_{name}.png");
            Load($"gun_player_{name}", $"Content/Assets/Tanks/gun_sand_{name}.png");
            Load($"gun_enemy_{name}", $"Content/Assets/Tanks/gun_olive_{name}.png");
            Load($"gun_ally_{name}", $"Content/Assets/Tanks/gun_teal_{name}.png");
            Load($"gun_neutral_{name}", $"Content/Assets/Tanks/gun_blue_{name}.png");
        }

        for (int i = 0; i <= 3; i++) Load($"terrain_sand_{i}", $"Content/Assets/Terrain/generated_sand_{i}.png");
        Load("terrain_road", "Content/Assets/Terrain/generated_road.png");
        Load("terrain_rock", "Content/Assets/Terrain/generated_rock.png");
        Load("terrain_water", "Content/Assets/Terrain/generated_water.png");
        for (int i = 1; i <= 17; i++) Load($"tile_{i}", $"Content/Assets/Terrain/tile_{i}.png");
        for (int i = 1; i <= 26; i++) LoadOptional($"road_{i}", $"Content/Assets/Terrain/rpg_road_{i}.png");
        LoadOptional("lake", "Content/Assets/Terrain/rpg_lake.png");
        for (int i = 1; i <= 20; i++) LoadOptional($"prop_{i}", $"Content/Assets/Props/object_{i}.png");
        for (int i = 1; i <= 8; i++) LoadOptional($"building_{i}", $"Content/Assets/Props/rpg_building_{i}.png");
        for (int i = 1; i <= 8; i++) LoadOptional($"decor_{i}", $"Content/Assets/Props/rpg_decor_{i}.png");

        for (int i = 0; i <= 8; i++) Load($"explosion_{i}", $"Content/Assets/Effects/sprite_effects_explosion_{i:000}.png");
        LoadOptional("shell_light", "Content/Assets/Effects/light_shell.png");
        LoadOptional("shell_medium", "Content/Assets/Effects/medium_shell.png");
        LoadOptional("flash_1", "Content/Assets/Effects/flash_a_01.png");
        LoadOptional("flash_2", "Content/Assets/Effects/flash_a_02.png");

        for (int color = 1; color <= 3; color++)
        {
            for (int boat = 1; boat <= 4; boat++)
            {
                LoadOptional($"boat_{color}_{boat}", $"Content/Assets/Boats/boats_color{color}_boat_color{color}_{boat}.png");
            }
        }

        for (int boat = 1; boat <= 4; boat++)
        {
            for (int color = 1; color <= 3; color++)
            {
                for (int frame = 1; frame <= 4; frame++)
                {
                    LoadOptional($"boat_water_{boat}_{color}_{frame}", $"Content/Assets/Boats/boat{boat}_water_animation_color{color}_boat{boat}_water_frame{frame}.png");
                }
            }
        }
    }

    private void LoadOptional(string key, string path)
    {
        try { Load(key, path); } catch { }
    }

    private void Load(string key, string path)
    {
        using var stream = TitleContainer.OpenStream(path);
        _textures[key] = Texture2D.FromStream(_graphics, stream);
    }

    public void Dispose()
    {
        foreach (var texture in _textures.Values) texture.Dispose();
        _textures.Clear();
    }
}
