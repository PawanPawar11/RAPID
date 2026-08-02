using System.Collections.Concurrent;
using RAPID.Storage.Models;

namespace RAPID.Storage;

public partial class Database
{
    private readonly ConcurrentDictionary<string, RedisValue> _store = new();

    private bool TryGetValidKey(string key, out RedisValue? redisVal)
    {
        if (_store.TryGetValue(key, out redisVal))
        {
            if (redisVal.IsExpired)
            {
                _store.TryRemove(key, out _);
                redisVal = null;
                return false;
            }
            return true;
        }
        redisVal = null;
        return false;
    }

    public bool Exists(string key)
    {
        return TryGetValidKey(key, out _);
    }

    public bool Del(string key)
    {
        if (!TryGetValidKey(key, out _))
            return false;

        return _store.TryRemove(key, out _);
    }
}
