using Microsoft.Xna.Framework;
using WorldOrder.Core;
using WorldOrder.Gameplay;

namespace WorldOrder.Entities;

public sealed class EntityManager
{
    private readonly List<Entity> _entities = new();
    private float _spawnTimer = 24f;

    public IEnumerable<Entity> All => _entities;
    public IEnumerable<Zombie> Zombies => _entities.OfType<Zombie>();
    public int ZombieCount => _entities.OfType<Zombie>().Count();

    public void Add(Entity entity) => _entities.Add(entity);

    public void Update(WorldSession session, GameTime gameTime)
    {
        var dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _spawnTimer -= dt;
        if (_spawnTimer <= 0f)
        {
            _spawnTimer = Balance.ZombieSpawnSeconds;
            SpawnZombieWave(session);
        }

        foreach (var entity in _entities.ToArray()) entity.Update(session, gameTime);
        _entities.RemoveAll(e => e.Removed || Vector2.DistanceSquared(e.Position, session.Player.Position) > 2600f * 2600f);
    }

    public void DropLoot(Vector2 position, IEnumerable<(ItemId Item, int Count)> drops)
    {
        var offset = 0;
        foreach (var drop in drops)
        {
            if (drop.Count <= 0) continue;
            var angle = offset * 1.8f;
            Add(new Pickup(position + new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * 16f, drop.Item, drop.Count));
            offset++;
        }
    }

    private void SpawnZombieWave(WorldSession session)
    {
        if (ZombieCount >= Balance.ZombieSoftCap) return;
        var seed = session.State.Seed + session.State.Day * 997 + (int)session.State.WorldTimeSeconds;
        if (session.State.Day == 1 && session.State.WorldTimeSeconds < 360f) return;
        var count = 1 + Math.Max(0, session.State.Day - 1) / 3;
        for (var i = 0; i < count; i++)
        {
            if (ZombieCount >= Balance.ZombieSoftCap) return;
            var pos = session.Generator.NextZombieSpawn(session.Player.Position, seed + i * 31);
            pos += new Vector2((Hashing.Unit(seed, i, 123) - 0.5f) * 64f, (Hashing.Unit(seed, i, 321) - 0.5f) * 64f);
            if (session.Chunks.IsBlocked(pos)) pos = session.Generator.NextZombieSpawn(session.Player.Position, seed + i * 101);
            if (session.Chunks.IsBlocked(pos)) continue;
            var tier = Hashing.Unit(seed, i, 777) > 0.88f ? ZombieTier.Brute : ZombieTier.Walker;
            Add(new Zombie(pos, tier));
        }
    }
}
