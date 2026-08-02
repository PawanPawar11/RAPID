using System;
using System.Collections.Generic;
using RAPID.Persistence;
using RAPID.Storage.Models;

namespace RAPID.Storage;

public partial class Database
{
    public List<KeySnapshotDto> CreateSnapshot()
    {
        var snapshots = new List<KeySnapshotDto>();
        DateTime now = DateTime.UtcNow;

        foreach (var kvp in _store)
        {
            var redisVal = kvp.Value;
            if (redisVal.IsExpired)
            {
                continue; // Skip expired keys
            }

            var dto = new KeySnapshotDto
            {
                Key = kvp.Key,
                Type = redisVal.Type,
                ExpiresAtUtc = redisVal.ExpiresAtUtc
            };

            switch (redisVal.Type)
            {
                case RedisDataType.String:
                    dto.StringValue = redisVal.StringData;
                    break;

                case RedisDataType.List:
                    lock (redisVal.ListData)
                    {
                        dto.ListValue = new List<string>(redisVal.ListData);
                    }
                    break;

                case RedisDataType.Hash:
                    lock (redisVal.HashData)
                    {
                        dto.HashValue = new Dictionary<string, string>(redisVal.HashData);
                    }
                    break;
            }

            snapshots.Add(dto);
        }

        return snapshots;
    }

    public int LoadSnapshot(List<KeySnapshotDto> keys)
    {
        _store.Clear();
        int loadedCount = 0;
        DateTime now = DateTime.UtcNow;

        foreach (var dto in keys)
        {
            if (dto.ExpiresAtUtc.HasValue && now >= dto.ExpiresAtUtc.Value)
            {
                continue; // Skip expired keys during load
            }

            RedisValue? redisVal = dto.Type switch
            {
                RedisDataType.String when dto.StringValue != null =>
                    new RedisValue(dto.StringValue, dto.ExpiresAtUtc),

                RedisDataType.List when dto.ListValue != null =>
                    new RedisValue(new LinkedList<string>(dto.ListValue), dto.ExpiresAtUtc),

                RedisDataType.Hash when dto.HashValue != null =>
                    new RedisValue(new Dictionary<string, string>(dto.HashValue), dto.ExpiresAtUtc),

                _ => null
            };

            if (redisVal != null)
            {
                _store[dto.Key] = redisVal;
                loadedCount++;
            }
        }

        return loadedCount;
    }
}
