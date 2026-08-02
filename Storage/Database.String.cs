using RAPID.Storage.Models;

namespace RAPID.Storage;

public partial class Database
{
    public void Set(string key, string value)
    {
        _store[key] = new RedisValue(value, expiresAtUtc: null);
    }

    public GetResult Get(string key)
    {
        if (TryGetValidKey(key, out var redisVal))
        {
            if (redisVal!.Type != RedisDataType.String)
            {
                return new GetResult(GetResultType.WrongType, null);
            }
            return new GetResult(GetResultType.Success, redisVal.StringData);
        }
        return new GetResult(GetResultType.NotFound, null);
    }
}
