using Microsoft.Xna.Framework;

namespace WorldOrder.Core;

public static class Balance
{
    public const int VirtualWidth = 1280;
    public const int VirtualHeight = 720;
    public const int TileSize = 32;
    public const int ChunkSize = 32;
    public const int ChunkLoadRadius = 3;
    public const int ChunkKeepRadius = 4;
    public const float PlayerWalkSpeed = 118f;
    public const float PlayerSprintSpeed = 176f;
    public const float PlayerInteractionRange = 48f;
    public const float PlayerAttackRange = 42f;
    public const float PlayerAttackSeconds = 0.42f;
    public const float ZombieSpawnSeconds = 5.5f;
    public const int ZombieSoftCap = 48;
    public const float ZombieSightRange = 380f;
    public const float ZombieAttackRange = 28f;
    public const float ZombieAttackSeconds = 0.92f;
    public const float DayLengthSeconds = 900f;
    public const int SaveVersion = 1;
    public static readonly Color ClearColor = new(11, 13, 14);
}
