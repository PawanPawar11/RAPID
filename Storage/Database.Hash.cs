using System.Collections.Generic;
using RAPID.Storage.Models;

namespace RAPID.Storage;

public partial class Database
{
    public HashResult HSet(string key, string field, string value)
    {
        while (true)
        {
            if (_store.TryGetValue(key, out var oldVal))
            {
                if (oldVal.IsExpired)
                {
                    _store.TryRemove(key, out _);
                    continue;
                }

                if (oldVal.Type != RedisDataType.Hash)
                {
                    return new HashResult(HashResultType.WrongType);
                }

                lock (oldVal.HashData)
                {
                    bool isNew = !oldVal.HashData.ContainsKey(field);
                    oldVal.HashData[field] = value;
                    return new HashResult(HashResultType.Success, IntegerReply: isNew ? 1 : 0);
                }
            }
            else
            {
                var newHash = new Dictionary<string, string> { [field] = value };
                var newVal = new RedisValue(newHash, expiresAtUtc: null);
                if (_store.TryAdd(key, newVal))
                {
                    return new HashResult(HashResultType.Success, IntegerReply: 1);
                }
            }
        }
    }

    public HashResult HGet(string key, string field)
    {
        if (_store.TryGetValue(key, out var oldVal))
        {
            if (oldVal.IsExpired)
            {
                _store.TryRemove(key, out _);
                return new HashResult(HashResultType.KeyNotFound);
            }

            if (oldVal.Type != RedisDataType.Hash)
            {
                return new HashResult(HashResultType.WrongType);
            }

            lock (oldVal.HashData)
            {
                if (oldVal.HashData.TryGetValue(field, out var value))
                {
                    return new HashResult(HashResultType.Success, Value: value);
                }
                return new HashResult(HashResultType.FieldNotFound);
            }
        }

        return new HashResult(HashResultType.KeyNotFound);
    }

    public HashResult HDel(string key, string field)
    {
        if (_store.TryGetValue(key, out var oldVal))
        {
            if (oldVal.IsExpired)
            {
                _store.TryRemove(key, out _);
                return new HashResult(HashResultType.KeyNotFound);
            }

            if (oldVal.Type != RedisDataType.Hash)
            {
                return new HashResult(HashResultType.WrongType);
            }

            lock (oldVal.HashData)
            {
                bool removed = oldVal.HashData.Remove(field);
                if (oldVal.HashData.Count == 0)
                {
                    _store.TryRemove(key, out _);
                }
                return new HashResult(HashResultType.Success, IntegerReply: removed ? 1 : 0);
            }
        }

        return new HashResult(HashResultType.KeyNotFound);
    }

    public HashResult HExists(string key, string field)
    {
        if (_store.TryGetValue(key, out var oldVal))
        {
            if (oldVal.IsExpired)
            {
                _store.TryRemove(key, out _);
                return new HashResult(HashResultType.Success, IntegerReply: 0);
            }

            if (oldVal.Type != RedisDataType.Hash)
            {
                return new HashResult(HashResultType.WrongType);
            }

            lock (oldVal.HashData)
            {
                bool exists = oldVal.HashData.ContainsKey(field);
                return new HashResult(HashResultType.Success, IntegerReply: exists ? 1 : 0);
            }
        }

        return new HashResult(HashResultType.Success, IntegerReply: 0);
    }

    public HashResult HGetAll(string key)
    {
        if (_store.TryGetValue(key, out var oldVal))
        {
            if (oldVal.IsExpired)
            {
                _store.TryRemove(key, out _);
                return new HashResult(HashResultType.Success, Entries: new Dictionary<string, string>());
            }

            if (oldVal.Type != RedisDataType.Hash)
            {
                return new HashResult(HashResultType.WrongType);
            }

            lock (oldVal.HashData)
            {
                var snapshot = new Dictionary<string, string>(oldVal.HashData);
                return new HashResult(HashResultType.Success, Entries: snapshot);
            }
        }

        return new HashResult(HashResultType.Success, Entries: new Dictionary<string, string>());
    }
}
