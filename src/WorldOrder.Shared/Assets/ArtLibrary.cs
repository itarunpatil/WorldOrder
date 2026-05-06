using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WorldOrder.Core;
using WorldOrder.World;

namespace WorldOrder.Assets;

public enum SpriteId
{
    PlayerIdleDown,
    PlayerRunDown,
    PlayerIdleSide,
    PlayerRunSide,
    PlayerIdleUp,
    PlayerRunUp,
    PlayerPunchDown,
    PlayerPunchSide,
    PlayerPunchUp,
    PlayerPistolDown,
    PlayerPistolSide,
    PlayerPistolUp,
    ZombieWalkDown,
    ZombieWalkSide,
    ZombieWalkUp,
    CannedFood,
    Scrap,
    Wood,
    Water,
    Medkit,
    Pistol,
    WallWood,
    WallReinforced,
    Campfire,
    Crate,
    Tree,
    Car,
    Barrel
}

public sealed class ArtLibrary
{
    private readonly Dictionary<string, Texture2D> _textures = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<SpriteId, SpriteSheet> _sheets = new();
    private readonly Dictionary<TileType, List<Texture2D>> _tileVariants = new();
    private readonly GraphicsDevice _graphicsDevice;

    public ArtLibrary(GraphicsDevice graphicsDevice)
    {
        _graphicsDevice = graphicsDevice;
        Pixel = new Texture2D(graphicsDevice, 1, 1);
        Pixel.SetData(new[] { Color.White });
    }

    public Texture2D Pixel { get; }
    public bool ExternalArtLoaded { get; private set; }

    public void Load()
    {
        CreateProceduralTileSet();
        CreateProceduralSprites();
        LoadPostApocalypsePack();
    }

    public Texture2D Tile(TileType type) => Tile(type, 0, 0);

    public Texture2D Tile(TileType type, int tileX, int tileY)
    {
        if (!_tileVariants.TryGetValue(type, out var list) || list.Count == 0) list = _tileVariants[TileType.Dirt];
        var index = list.Count == 1 ? 0 : (int)(Hashing.Hash2(tileX, tileY, 63000 + (int)type * 97) % (uint)list.Count);
        return list[index];
    }

    public Texture2D Texture(string key) => _textures.TryGetValue(key, out var texture) ? texture : _textures["crate"];
    public bool TrySheet(SpriteId id, out SpriteSheet sheet) => _sheets.TryGetValue(id, out sheet!);

    private void CreateProceduralTileSet()
    {
        SetTile(TileType.Dirt, CreateTile(new Color(78, 69, 55), new Color(99, 84, 61), 11));
        SetTile(TileType.DryGrass, CreateTile(new Color(74, 85, 51), new Color(104, 109, 63), 13));
        SetTile(TileType.Rubble, CreateTile(new Color(70, 72, 69), new Color(112, 111, 104), 17));
        SetTile(TileType.Asphalt, CreateTile(new Color(42, 43, 44), new Color(58, 59, 60), 23));
        SetTile(TileType.Pavement, CreateTile(new Color(91, 92, 87), new Color(120, 118, 109), 29));
        SetTile(TileType.BuildingFloor, CreateTile(new Color(74, 70, 66), new Color(102, 92, 84), 31));
        SetTile(TileType.BuildingWall, CreateTile(new Color(52, 48, 45), new Color(85, 76, 66), 37));
        SetTile(TileType.Water, CreateTile(new Color(31, 58, 65), new Color(41, 90, 99), 41));
    }

    private void CreateProceduralSprites()
    {
        _textures["player"] = CreateSprite(18, 22, new Color(68, 90, 101), new Color(190, 158, 118), true);
        _textures["zombie"] = CreateSprite(18, 22, new Color(64, 111, 65), new Color(104, 61, 57), true);
        _textures["tree"] = CreateSprite(28, 38, new Color(68, 91, 43), new Color(83, 56, 35), false);
        _textures["car"] = CreateVehicle(new Color(88, 92, 87), new Color(42, 43, 44));
        _textures["barrel"] = CreateSprite(14, 18, new Color(128, 62, 45), new Color(62, 53, 48), false);
        _textures["crate"] = CreateSprite(18, 18, new Color(116, 88, 52), new Color(68, 50, 35), false);
        _textures["wood"] = CreateIcon(new Color(122, 86, 49), new Color(72, 47, 28));
        _textures["scrap"] = CreateIcon(new Color(139, 141, 137), new Color(71, 73, 72));
        _textures["food"] = CreateIcon(new Color(178, 69, 47), new Color(230, 185, 90));
        _textures["water"] = CreateIcon(new Color(61, 126, 171), new Color(139, 190, 218));
        _textures["medkit"] = CreateIcon(new Color(220, 220, 210), new Color(172, 42, 42));
        _textures["pistol"] = CreateIcon(new Color(50, 51, 55), new Color(166, 166, 160));
        _textures["wallwood"] = CreateSprite(32, 32, new Color(95, 66, 42), new Color(52, 38, 30), false);
        _textures["wallreinforced"] = CreateSprite(32, 32, new Color(93, 96, 95), new Color(54, 57, 56), false);
        _textures["campfire"] = CreateIcon(new Color(210, 98, 38), new Color(100, 56, 30));
        _textures["grass1"] = CreateIcon(new Color(83, 116, 62), new Color(57, 79, 42));
        _textures["grass2"] = _textures["grass1"];
        _textures["bush"] = CreateSprite(20, 18, new Color(57, 96, 53), new Color(42, 63, 36), false);
        _textures["tires"] = CreateIcon(new Color(45, 47, 48), new Color(90, 94, 88));
        _textures["cardboard"] = CreateIcon(new Color(140, 103, 63), new Color(80, 59, 38));
        _textures["garbagebin"] = CreateIcon(new Color(48, 92, 67), new Color(28, 49, 39));
        _textures["hydrant"] = CreateIcon(new Color(170, 48, 39), new Color(78, 38, 33));
        _textures["manhole"] = CreateIcon(new Color(78, 82, 84), new Color(38, 40, 42));
        _textures["bench"] = CreateSprite(26, 16, new Color(109, 70, 42), new Color(51, 49, 45), false);
        _textures["container"] = CreateSprite(34, 42, new Color(90, 99, 95), new Color(48, 53, 51), false);
        _textures["airvent"] = CreateIcon(new Color(106, 112, 112), new Color(60, 64, 64));
        _textures["door"] = CreateIcon(new Color(118, 95, 67), new Color(58, 45, 34));
        _textures["destroyedwall"] = CreateSprite(22, 20, new Color(92, 83, 74), new Color(52, 47, 43), false);
        _textures["brickdebris"] = CreateIcon(new Color(104, 76, 62), new Color(60, 50, 44));
        _textures["roofhole"] = CreateIcon(new Color(28, 28, 27), new Color(78, 74, 70));
        _textures["fence"] = CreateIcon(new Color(118, 125, 122), new Color(67, 72, 70));
        _textures["blood"] = CreateBloodSplat();
        _textures["slash"] = CreateSlash();
    }

    private void LoadPostApocalypsePack()
    {
        var loaded = 0;

        loaded += LoadSheet(SpriteId.PlayerIdleDown, "Character/Main/Idle/Character_down_idle-Sheet6.png", 6, 0.14f);
        loaded += LoadSheet(SpriteId.PlayerRunDown, "Character/Main/Run/Character_down_run-Sheet6.png", 6, 0.09f);
        loaded += LoadSheet(SpriteId.PlayerIdleSide, "Character/Main/Idle/Character_side_idle-Sheet6.png", 6, 0.14f);
        loaded += LoadSheet(SpriteId.PlayerRunSide, "Character/Main/Run/Character_side_run-Sheet6.png", 6, 0.09f);
        loaded += LoadSheet(SpriteId.PlayerIdleUp, "Character/Main/Idle/Character_up_idle-Sheet6.png", 6, 0.14f);
        loaded += LoadSheet(SpriteId.PlayerRunUp, "Character/Main/Run/Character_up_run-Sheet6.png", 6, 0.09f);
        loaded += LoadSheet(SpriteId.PlayerPunchDown, "Character/Main/Punch/Character_down_punch-Sheet4.png", 4, 0.055f);
        loaded += LoadSheet(SpriteId.PlayerPunchSide, "Character/Main/Punch/Character_side_punch-Sheet4.png", 4, 0.055f);
        loaded += LoadSheet(SpriteId.PlayerPunchUp, "Character/Main/Punch/Character_up_punch-Sheet4.png", 4, 0.055f);
        loaded += LoadSheet(SpriteId.PlayerPistolDown, "Character/Guns/Pistol/Pistol_down_shoot-Sheet3.png", 3, 0.050f);
        loaded += LoadSheet(SpriteId.PlayerPistolSide, "Character/Guns/Pistol/Pistol_side_shoot-Sheet3.png", 3, 0.050f);
        loaded += LoadSheet(SpriteId.PlayerPistolUp, "Character/Guns/Pistol/Pistol_up_shoot-Sheet3.png", 3, 0.050f);
        loaded += LoadSheet(SpriteId.ZombieWalkDown, "Enemies/Zombie_Small/Zombie_Small_Down_walk-Sheet6.png", 6, 0.12f);
        loaded += LoadSheet(SpriteId.ZombieWalkSide, "Enemies/Zombie_Small/Zombie_Small_Side_Walk-Sheet6.png", 6, 0.12f);
        loaded += LoadSheet(SpriteId.ZombieWalkUp, "Enemies/Zombie_Small/Zombie_Small_Up_Walk-Sheet6.png", 6, 0.12f);

        loaded += LoadTexture("food", "Objects/Pickable/Canned-food.png");
        loaded += LoadTexture("water", "Objects/Pickable/Canned-soup.png");
        loaded += LoadTexture("medkit", "Objects/Pickable/Bandage.png");
        loaded += LoadTexture("pistol", "Objects/Pickable/Pistol.png");
        loaded += LoadTexture("wood", "UI/Inventory/Objects/Icon_Wooden-wall.png");
        loaded += LoadTexture("scrap", "Objects/Vehicles/Rust/Car_6_Rust_Scrap/Car_6_Rust_Blue_Scrap.png");
        loaded += LoadTexture("crate", "Objects/Pickable/Ammo-crate_Green.png");
        loaded += LoadTexture("wallwood", "Objects/Buildable/Wooden/Wooden-wall_Horizontal.png");
        loaded += LoadTexture("wallreinforced", "Objects/Buildable/Reinforced/Reinforced_wooden-wall_Horizontal.png");
        loaded += LoadTexture("tree", "Objects/Nature/Green/Tree_1_Spruce_Green.png");
        loaded += LoadTexture("tree2", "Objects/Nature/Green/Tree_3_Normal_Green.png");
        loaded += LoadTexture("tree3", "Objects/Nature/Dark-Green/Tree_5_Big_Dark-Green.png");
        loaded += LoadTexture("stump", "Objects/Nature/Green/Tree-trunk_2_grass_Green.png");
        loaded += LoadTexture("rock", "Objects/Nature/Green/Rocks/Rock-grass.png");
        loaded += LoadTexture("car", "Objects/Vehicles/Rust/Car_1_Rust/Car_1_Rust_Red.png");
        loaded += LoadTexture("car2", "Objects/Vehicles/Overgrown/Car_4_Overgrown/Green/Car_4_Overgrown_Green_Blue.png");
        loaded += LoadTexture("car3", "Objects/Vehicles/Rust/Car_3_Rust_Van/Car_3_Rust_Blue_Van.png");
        loaded += LoadTexture("truck", "Objects/Vehicles/Overgrown/Car_7_Overgrown_Truck/Green/Car_7_Overgrown_Green_Dark-Blue_Truck.png");
        loaded += LoadTexture("barrel", "Objects/Barrel_rust_red_1.png");
        loaded += LoadTexture("barrel2", "Objects/Barrel_rust_blue_1.png");

        loaded += LoadTexture("grass1", "Objects/Nature/Green/Grass_1_Green.png");
        loaded += LoadTexture("grass2", "Objects/Nature/Green/Grass_2_Green.png");
        loaded += LoadTexture("grass3", "Objects/Nature/Green/Grass_5_Green.png");
        loaded += LoadTexture("bush", "Objects/Nature/Green/Bush_1_Green.png");
        loaded += LoadTexture("bush2", "Objects/Nature/Dark-Green/Bush_2_Dark-Green.png");
        loaded += LoadTexture("tires", "Objects/2-Tires_Grass_Green.png");
        loaded += LoadTexture("cardboard", "Objects/Cardboard_1.png");
        loaded += LoadTexture("cardboard2", "Objects/Cardboard_2.png");
        loaded += LoadTexture("garbagebin", "Objects/Garbage-Bin_1.png");
        loaded += LoadTexture("garbagebin2", "Objects/Garbage-Bin_3.png");
        loaded += LoadTexture("hydrant", "Objects/Hydrant_1_red.png");
        loaded += LoadTexture("hydrant2", "Objects/Hydrant_1_yellow.png");
        loaded += LoadTexture("manhole", "Objects/Manhole.png");
        loaded += LoadTexture("bench", "Objects/Bench_1_down.png");
        loaded += LoadTexture("bench2", "Objects/Bench_2_down_Overgrown_Green.png");
        loaded += LoadTexture("container", "Objects/Container/Container_1_Gray_Vertical.png");
        loaded += LoadTexture("container2", "Objects/Container/Container_3_Gray_Horizontal.png");
        loaded += LoadTexture("container3", "Objects/Container/Container_8_Red_Horizontal_Overgrown_Green.png");
        loaded += LoadTexture("airvent", "Objects/Buildings/Air-vent_1.png");
        loaded += LoadTexture("airvent2", "Objects/Buildings/HVAC_Overgrown_Green.png");
        loaded += LoadTexture("door", "Objects/Buildings/Door_1_Beige.png");
        loaded += LoadTexture("door2", "Objects/Buildings/Door_3_Boarded-up_Beige.png");
        loaded += LoadTexture("poster", "Objects/Buildings/layered-posters_1_For-ground-and-walls.png");
        loaded += LoadTexture("destroyedwall", "Objects/Buildings/Destroyed-wall_corner.png");
        loaded += LoadTexture("destroyedwall2", "Objects/Buildings/Destroyed-wall_not-corner.png");
        loaded += LoadTexture("brickdebris", "Objects/Gray-brick_Debris.png");
        loaded += LoadTexture("metalplates", "Objects/Metal-Plates.png");
        loaded += LoadTexture("roofhole", "Objects/Buildings/Roof-hole_1_Gray.png");
        loaded += LoadTexture("roofhole2", "Objects/Buildings/Roof-hole_2_Red.png");
        loaded += LoadTexture("fence", "Tiles/Wire-Fence/Wire-Fence_Gate.png");
        loaded += LoadTexture("ui_hp_back", "UI/HP/HP-Bar.png");
        loaded += LoadTexture("ui_hp_fill", "UI/HP/HP.png");
        loaded += LoadTexture("ui_hunger_back", "UI/Hunger/Hunger-Bar.png");
        loaded += LoadTexture("ui_hunger_fill", "UI/Hunger/Hunger.png");
        loaded += LoadTexture("ui_quickbar", "UI/Inventory/Quick-Access-Inventory.png");
        loaded += LoadTexture("ui_slot", "UI/Inventory/Inventory-Cell.png");
        loaded += LoadTexture("ui_slot_selected", "UI/Inventory/Inventory-Chosen.png");
        loaded += LoadTexture("ui_crafting", "UI/Crafting/Crafting-main-menu.png");
        loaded += LoadTexture("ui_crafting_slot", "UI/Crafting/Crafting-cell.png");
        loaded += LoadTexture("icon_food", "UI/Inventory/Objects/Icon_Canned-food.png");
        loaded += LoadTexture("icon_water", "UI/Inventory/Objects/Icon_Canned-soup.png");
        loaded += LoadTexture("icon_bandage", "UI/Inventory/Objects/Icon_Bandage.png");
        loaded += LoadTexture("icon_ammo", "UI/Inventory/Objects/Icon_Bullet-box_Green.png");
        loaded += LoadTexture("icon_pistol", "UI/Inventory/Objects/Icon_Pistol.png");
        loaded += LoadTexture("icon_woodwall", "UI/Inventory/Objects/Icon_Wooden-wall.png");
        loaded += LoadTexture("icon_reinforced", "UI/Inventory/Objects/Icon_Reinforced-wooden-wall.png");
        loaded += LoadTexture("icon_rock", "UI/Inventory/Objects/Icon_Rock.png");
        loaded += LoadTexture("icon_scrap", "Objects/Vehicles/Normal/Car_6_Scrap/Car_6_Gray_Scrap.png");
        loaded += LoadTexture("icon_cloth", "Objects/Cardboard_2.png");

        // Phase 4: use only clean base tiles for terrain. Road stripes, crosswalks, curbs,
        // and ruin detail are renderer/decorator overlays, never random ground variants.
        loaded += ReplaceTileFromSheet(TileType.DryGrass, "Tiles/Background_Green_TileSet.png", 0, 0);
        loaded += AddTileVariantFromSheet(TileType.DryGrass, "Tiles/Background_Green_TileSet.png", 2, 0);
        loaded += ReplaceTileFromSheet(TileType.Dirt, "Tiles/Background_Bleak-Yellow_TileSet.png", 0, 0);
        loaded += AddTileVariantFromSheet(TileType.Dirt, "Tiles/Background_Bleak-Yellow_TileSet.png", 1, 0);
        loaded += ReplaceTileFromSheet(TileType.Rubble, "Tiles/Garbage_TileSet.png", 2, 0);
        loaded += AddTileVariantFromSheet(TileType.Rubble, "Tiles/Garbage_TileSet.png", 1, 0);
        loaded += ReplaceTileFromSheet(TileType.Asphalt, "Tiles/Background_Green_TileSet.png", 0, 7);
        loaded += ReplaceTileFromSheet(TileType.Pavement, "Tiles/Background_Green_TileSet.png", 1, 1);
        loaded += ReplaceTileFromSheet(TileType.BuildingFloor, "Tiles/Buildings/Buildings_gray_TileSet.png", 7, 4);
        loaded += AddTileVariantFromSheet(TileType.BuildingFloor, "Tiles/Buildings/Buildings_gray_TileSet.png", 8, 4);
        loaded += ReplaceTileFromSheet(TileType.BuildingWall, "Tiles/Brick-Wall_TileSet.png", 0, 0);
        loaded += AddTileVariantFromSheet(TileType.BuildingWall, "Tiles/Brick-Wall_TileSet.png", 5, 0);

        ExternalArtLoaded = loaded > 0;
    }

    private int LoadSheet(SpriteId id, string path, int frames, float secondsPerFrame)
    {
        using var stream = AssetStorage.OpenPostApocalypseAsset(path);
        if (stream is null) return 0;
        var texture = Texture2D.FromStream(_graphicsDevice, stream);
        _sheets[id] = new SpriteSheet(texture, frames, secondsPerFrame);
        return 1;
    }

    private int LoadTexture(string key, string path)
    {
        using var stream = AssetStorage.OpenPostApocalypseAsset(path);
        if (stream is null) return 0;
        _textures[key] = Texture2D.FromStream(_graphicsDevice, stream);
        return 1;
    }

    private int ReplaceTileFromSheet(TileType type, string path, int tileX, int tileY)
    {
        var texture = LoadTileTexture(path, tileX, tileY);
        if (texture is null) return 0;
        SetTile(type, texture);
        return 1;
    }

    private int AddTileVariantFromSheet(TileType type, string path, int tileX, int tileY)
    {
        var texture = LoadTileTexture(path, tileX, tileY);
        if (texture is null) return 0;
        AddTileVariant(type, texture);
        return 1;
    }

    private Texture2D? LoadTileTexture(string path, int tileX, int tileY)
    {
        using var stream = AssetStorage.OpenPostApocalypseAsset(path);
        if (stream is null) return null;
        using var source = Texture2D.FromStream(_graphicsDevice, stream);
        var rect = new Rectangle(tileX * 16, tileY * 16, 16, 16);
        if (rect.Right > source.Width || rect.Bottom > source.Height) return null;
        return Crop(source, rect);
    }

    private void SetTile(TileType type, Texture2D texture)
    {
        _tileVariants[type] = new List<Texture2D> { texture };
    }

    private void AddTileVariant(TileType type, Texture2D texture)
    {
        if (!_tileVariants.TryGetValue(type, out var list))
        {
            list = new List<Texture2D>();
            _tileVariants[type] = list;
        }
        list.Add(texture);
    }

    private Texture2D Crop(Texture2D source, Rectangle rect)
    {
        var data = new Color[rect.Width * rect.Height];
        source.GetData(0, rect, data, 0, data.Length);
        var texture = new Texture2D(_graphicsDevice, rect.Width, rect.Height);
        texture.SetData(data);
        return texture;
    }

    private Texture2D CreateTile(Color primary, Color secondary, int salt)
    {
        const int size = 16;
        var data = new Color[size * size];
        for (var y = 0; y < size; y++)
        {
            for (var x = 0; x < size; x++)
            {
                var h = Hashing.Unit(x, y, salt);
                var color = h > 0.74f ? secondary : primary;
                if ((x == 0 || y == 0) && h > 0.45f) color = Color.Lerp(primary, Color.Black, 0.16f);
                data[y * size + x] = color;
            }
        }
        var texture = new Texture2D(_graphicsDevice, size, size);
        texture.SetData(data);
        return texture;
    }

    private Texture2D CreateSprite(int width, int height, Color body, Color accent, bool humanoid)
    {
        var data = new Color[width * height];
        Array.Fill(data, Color.Transparent);
        void Put(int x, int y, Color color)
        {
            if (x >= 0 && x < width && y >= 0 && y < height) data[y * width + x] = color;
        }

        if (humanoid)
        {
            for (var y = 3; y < 8; y++) for (var x = 6; x < 12; x++) Put(x, y, accent);
            for (var y = 8; y < 16; y++) for (var x = 5; x < 13; x++) Put(x, y, body);
            for (var y = 11; y < 20; y++) { Put(4, y, body); Put(13, y, body); }
            for (var y = 15; y < 22; y++) { Put(6, y, Color.Lerp(body, Color.Black, 0.25f)); Put(11, y, Color.Lerp(body, Color.Black, 0.25f)); }
            Put(7, 5, Color.Black); Put(10, 5, Color.Black);
        }
        else
        {
            for (var y = 3; y < height - 2; y++) for (var x = 3; x < width - 3; x++) if ((x + y) % 5 != 0) Put(x, y, body);
            for (var y = height / 2; y < height - 1; y++) Put(width / 2, y, accent);
        }
        var texture = new Texture2D(_graphicsDevice, width, height);
        texture.SetData(data);
        return texture;
    }

    private Texture2D CreateVehicle(Color body, Color shadow)
    {
        var w = 38;
        var h = 22;
        var data = new Color[w * h];
        Array.Fill(data, Color.Transparent);
        void Rect(int x, int y, int rw, int rh, Color color)
        {
            for (var yy = y; yy < y + rh; yy++) for (var xx = x; xx < x + rw; xx++) if (xx >= 0 && yy >= 0 && xx < w && yy < h) data[yy * w + xx] = color;
        }
        Rect(4, 6, 30, 10, body);
        Rect(10, 3, 16, 7, Color.Lerp(body, Color.White, 0.15f));
        Rect(6, 15, 7, 4, shadow);
        Rect(25, 15, 7, 4, shadow);
        var texture = new Texture2D(_graphicsDevice, w, h);
        texture.SetData(data);
        return texture;
    }

    private Texture2D CreateIcon(Color primary, Color accent)
    {
        const int size = 16;
        var data = new Color[size * size];
        Array.Fill(data, Color.Transparent);
        for (var y = 3; y < 13; y++) for (var x = 3; x < 13; x++) data[y * size + x] = primary;
        for (var i = 0; i < size; i++)
        {
            data[3 * size + i] = accent;
            data[12 * size + i] = Color.Lerp(accent, Color.Black, 0.2f);
        }
        var texture = new Texture2D(_graphicsDevice, size, size);
        texture.SetData(data);
        return texture;
    }

    private Texture2D CreateBloodSplat()
    {
        const int size = 24;
        var data = new Color[size * size];
        Array.Fill(data, Color.Transparent);
        for (var y = 0; y < size; y++)
        {
            for (var x = 0; x < size; x++)
            {
                var dx = x - 12;
                var dy = y - 12;
                var r = MathF.Sqrt(dx * dx + dy * dy);
                if (r < 7f || (Hashing.Unit(x, y, 913) > 0.83f && r < 11f)) data[y * size + x] = new Color(102, 18, 16, 210);
            }
        }
        var texture = new Texture2D(_graphicsDevice, size, size);
        texture.SetData(data);
        return texture;
    }

    private Texture2D CreateSlash()
    {
        const int size = 32;
        var data = new Color[size * size];
        Array.Fill(data, Color.Transparent);
        for (var y = 0; y < size; y++)
        {
            for (var x = 0; x < size; x++)
            {
                var d = Math.Abs(y - (x / 2 + 7));
                if (d < 2 && x > 8 && x < 28) data[y * size + x] = new Color(240, 238, 196, 210);
            }
        }
        var texture = new Texture2D(_graphicsDevice, size, size);
        texture.SetData(data);
        return texture;
    }
}
