using System;

namespace RAPID.Storage;

public class RedisValue
{
    public string Value { get; set; }
    public DateTime? ExpiresAtUtc { get; set; }

    public RedisValue(string value, DateTime? expiresAtUtc = null)
    {
        Value = value;
        ExpiresAtUtc = expiresAtUtc;
    }

    public bool IsExpired => ExpiresAtUtc.HasValue && DateTime.UtcNow >= ExpiresAtUtc.Value;
}
