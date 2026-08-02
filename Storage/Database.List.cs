using System.Collections.Generic;
using RAPID.Storage.Models;

namespace RAPID.Storage;

public partial class Database
{
    public ListResult LPush(string key, IEnumerable<string> values)
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

                if (oldVal.Type != RedisDataType.List)
                {
                    return new ListResult(ListResultType.WrongType);
                }

                lock (oldVal.ListData)
                {
                    foreach (var val in values)
                    {
                        oldVal.ListData.AddFirst(val);
                    }
                    return new ListResult(ListResultType.Success, oldVal.ListData.Count);
                }
            }
            else
            {
                var newList = new LinkedList<string>();
                foreach (var val in values)
                {
                    newList.AddFirst(val);
                }
                var newVal = new RedisValue(newList, expiresAtUtc: null);
                if (_store.TryAdd(key, newVal))
                {
                    return new ListResult(ListResultType.Success, newList.Count);
                }
            }
        }
    }

    public ListResult RPush(string key, IEnumerable<string> values)
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

                if (oldVal.Type != RedisDataType.List)
                {
                    return new ListResult(ListResultType.WrongType);
                }

                lock (oldVal.ListData)
                {
                    foreach (var val in values)
                    {
                        oldVal.ListData.AddLast(val);
                    }
                    return new ListResult(ListResultType.Success, oldVal.ListData.Count);
                }
            }
            else
            {
                var newList = new LinkedList<string>();
                foreach (var val in values)
                {
                    newList.AddLast(val);
                }
                var newVal = new RedisValue(newList, expiresAtUtc: null);
                if (_store.TryAdd(key, newVal))
                {
                    return new ListResult(ListResultType.Success, newList.Count);
                }
            }
        }
    }

    public ListResult LPop(string key)
    {
        if (_store.TryGetValue(key, out var oldVal))
        {
            if (oldVal.IsExpired)
            {
                _store.TryRemove(key, out _);
                return new ListResult(ListResultType.KeyNotFound);
            }

            if (oldVal.Type != RedisDataType.List)
            {
                return new ListResult(ListResultType.WrongType);
            }

            lock (oldVal.ListData)
            {
                if (oldVal.ListData.Count == 0)
                {
                    _store.TryRemove(key, out _);
                    return new ListResult(ListResultType.KeyNotFound);
                }

                string popped = oldVal.ListData.First!.Value;
                oldVal.ListData.RemoveFirst();

                if (oldVal.ListData.Count == 0)
                {
                    _store.TryRemove(key, out _);
                }

                return new ListResult(ListResultType.Success, PopValue: popped);
            }
        }

        return new ListResult(ListResultType.KeyNotFound);
    }

    public ListResult RPop(string key)
    {
        if (_store.TryGetValue(key, out var oldVal))
        {
            if (oldVal.IsExpired)
            {
                _store.TryRemove(key, out _);
                return new ListResult(ListResultType.KeyNotFound);
            }

            if (oldVal.Type != RedisDataType.List)
            {
                return new ListResult(ListResultType.WrongType);
            }

            lock (oldVal.ListData)
            {
                if (oldVal.ListData.Count == 0)
                {
                    _store.TryRemove(key, out _);
                    return new ListResult(ListResultType.KeyNotFound);
                }

                string popped = oldVal.ListData.Last!.Value;
                oldVal.ListData.RemoveLast();

                if (oldVal.ListData.Count == 0)
                {
                    _store.TryRemove(key, out _);
                }

                return new ListResult(ListResultType.Success, PopValue: popped);
            }
        }

        return new ListResult(ListResultType.KeyNotFound);
    }

    public ListResult LLen(string key)
    {
        if (_store.TryGetValue(key, out var oldVal))
        {
            if (oldVal.IsExpired)
            {
                _store.TryRemove(key, out _);
                return new ListResult(ListResultType.Success, Length: 0);
            }

            if (oldVal.Type != RedisDataType.List)
            {
                return new ListResult(ListResultType.WrongType);
            }

            lock (oldVal.ListData)
            {
                return new ListResult(ListResultType.Success, Length: oldVal.ListData.Count);
            }
        }

        return new ListResult(ListResultType.Success, Length: 0);
    }
}
