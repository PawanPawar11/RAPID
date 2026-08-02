using System;
using RAPID.Storage.Models;

namespace RAPID.Storage;

public partial class Database
{
    public NumericResult IncrBy(string key, long amount)
    {
        while (true)
        {
            if (_store.TryGetValue(key, out var oldVal))
            {
                if (oldVal.IsExpired)
                {
                    _store.TryRemove(key, out _);
                    var newVal = new RedisValue(amount.ToString(), null);
                    if (_store.TryAdd(key, newVal))
                    {
                        return new NumericResult(NumericResultType.Success, amount);
                    }
                    continue;
                }

                if (oldVal.Type != RedisDataType.String)
                {
                    return new NumericResult(NumericResultType.WrongType, 0);
                }

                if (!long.TryParse(oldVal.StringData, out long current))
                {
                    return new NumericResult(NumericResultType.NotAnInteger, 0);
                }

                try
                {
                    long newValue = checked(current + amount);
                    var newVal = new RedisValue(newValue.ToString(), oldVal.ExpiresAtUtc);
                    if (_store.TryUpdate(key, newVal, oldVal))
                    {
                        return new NumericResult(NumericResultType.Success, newValue);
                    }
                }
                catch (OverflowException)
                {
                    return new NumericResult(NumericResultType.Overflow, 0);
                }
            }
            else
            {
                var newVal = new RedisValue(amount.ToString(), null);
                if (_store.TryAdd(key, newVal))
                {
                    return new NumericResult(NumericResultType.Success, amount);
                }
            }
        }
    }
}
