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
    private readonly Dictionary<TileType, Texture2D> _tiles = new();
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

    public Texture2D Tile(TileType type) => _tiles.TryGetValue(type, out var texture) ? texture : _tiles[TileType.Dirt];
    public Texture2D Texture(string key) => _textures[key];
    public bool TrySheet(SpriteId id, out SpriteSheet sheet) => _sheets.TryGetValue(id, out sheet!);

    private void CreateProceduralTileSet()
    {
        _tiles[TileType.Dirt] = CreateTile(new Color(82, 69, 52), new Color(70, 58, 44), 11);
        _tiles[TileType.DryGrass] = CreateTile(new Color(80, 84, 44), new Color(111, 105, 53), 13);
        _tiles[TileType.Rubble] = CreateTile(new Color(70, 72, 69), new Color(104, 105, 100), 17);
        _tiles[TileType.Asphalt] = CreateTile(new Color(42, 43, 44), new Color(58, 59, 60), 23);
        _tiles[TileType.Pavement] = CreateTile(new Color(91, 92, 87), new Color(120, 118, 109), 29);
        _tiles[TileType.BuildingFloor] = CreateTile(new Color(74, 70, 66), new Color(102, 92, 84), 31);
        _tiles[TileType.BuildingWall] = CreateTile(new Color(52, 48, 45), new Color(85, 76, 66), 37);
        _tiles[TileType.Water] = CreateTile(new Color(31, 58, 65), new Color(41, 90, 99), 41);
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
        loaded += LoadTexture("car", "Objects/Vehicles/Rust/Car_1_Rust/Car_1_Rust_Red.png");
        loaded += LoadTexture("barrel", "Objects/Barrel_rust_red_1.png");

        loaded += LoadTileFromSheet(TileType.DryGrass, "Tiles/Background_Green_TileSet.png", 0, 0);
        loaded += LoadTileFromSheet(TileType.Dirt, "Tiles/Background_Bleak-Yellow_TileSet.png", 7, 5);
        loaded += LoadTileFromSheet(TileType.Rubble, "Tiles/Garbage_TileSet.png", 0, 0);
        loaded += LoadTileFromSheet(TileType.Asphalt, "Tiles/Background_Green_TileSet.png", 0, 8);
        loaded += LoadTileFromSheet(TileType.Pavement, "Tiles/Background_Green_TileSet.png", 0, 10);
        loaded += LoadTileFromSheet(TileType.BuildingFloor, "Tiles/Buildings/Buildings_gray_TileSet.png", 1, 0);
        loaded += LoadTileFromSheet(TileType.BuildingWall, "Tiles/Brick-Wall_TileSet.png", 0, 0);

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

    private int LoadTileFromSheet(TileType type, string path, int tileX, int tileY)
    {
        using var stream = AssetStorage.OpenPostApocalypseAsset(path);
        if (stream is null) return 0;
        using var source = Texture2D.FromStream(_graphicsDevice, stream);
        var rect = new Rectangle(tileX * 16, tileY * 16, 16, 16);
        if (rect.Right > source.Width || rect.Bottom > source.Height) return 0;
        _tiles[type] = Crop(source, rect);
        return 1;
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
}
