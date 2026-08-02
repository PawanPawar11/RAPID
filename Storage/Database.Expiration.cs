using System;

namespace RAPID.Storage;

public partial class Database
{
    public int Expire(string key, int seconds)
    {
        if (!TryGetValidKey(key, out var redisVal))
            return 0;

        if (seconds <= 0)
        {
            _store.TryRemove(key, out _);
            return 1;
        }

        redisVal!.ExpiresAtUtc = DateTime.UtcNow.AddSeconds(seconds);
        return 1;
    }

    public long Ttl(string key)
    {
        if (!_store.TryGetValue(key, out var redisVal))
            return -2;

        if (redisVal.IsExpired)
        {
            _store.TryRemove(key, out _);
            return -2;
        }

        if (!redisVal.ExpiresAtUtc.HasValue)
            return -1;

        TimeSpan remaining = redisVal.ExpiresAtUtc.Value - DateTime.UtcNow;
        long seconds = (long)Math.Ceiling(remaining.TotalSeconds);
        return seconds > 0 ? seconds : -2;
    }

    public int CleanupExpiredKeys()
    {
        int count = 0;
        foreach (var kvp in _store)
        {
            if (kvp.Value.IsExpired)
            {
                if (_store.TryRemove(kvp.Key, out _))
                {
                    count++;
                }
            }
        }
        return count;
    }
}
