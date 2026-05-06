namespace WorldOrder.Core;

public static class Hashing
{
    public static int StableStringHash(string text)
    {
        unchecked
        {
            var hash = 2166136261u;
            foreach (var ch in text)
            {
                hash ^= ch;
                hash *= 16777619u;
            }
            return (int)hash;
        }
    }

    public static uint Hash2(int x, int y, int seed)
    {
        unchecked
        {
            var h = (uint)seed;
            h ^= (uint)x + 0x9E3779B9u + (h << 6) + (h >> 2);
            h ^= (uint)y + 0x85EBCA6Bu + (h << 6) + (h >> 2);
            h ^= h >> 16;
            h *= 0x7FEB352Du;
            h ^= h >> 15;
            h *= 0x846CA68Bu;
            h ^= h >> 16;
            return h;
        }
    }

    public static float Unit(int x, int y, int seed)
    {
        return (Hash2(x, y, seed) & 0x00FFFFFF) / 16777215f;
    }

    public static int Range(int x, int y, int seed, int minInclusive, int maxExclusive)
    {
        var span = maxExclusive - minInclusive;
        if (span <= 0) return minInclusive;
        return minInclusive + (int)(Hash2(x, y, seed) % (uint)span);
    }
}
