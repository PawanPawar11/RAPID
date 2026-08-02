using System;
using System.Collections.Generic;

namespace RAPID.Storage;

public class RedisValue
{
    public RedisDataType Type { get; set; }
    public object Data { get; set; }
    public DateTime? ExpiresAtUtc { get; set; }

    public RedisValue(string stringValue, DateTime? expiresAtUtc = null)
    {
        Type = RedisDataType.String;
        Data = stringValue;
        ExpiresAtUtc = expiresAtUtc;
    }

    public RedisValue(LinkedList<string> listValue, DateTime? expiresAtUtc = null)
    {
        Type = RedisDataType.List;
        Data = listValue;
        ExpiresAtUtc = expiresAtUtc;
    }

    public string StringData => (string)Data;
    public LinkedList<string> ListData => (LinkedList<string>)Data;

    public bool IsExpired => ExpiresAtUtc.HasValue && DateTime.UtcNow >= ExpiresAtUtc.Value;
}
