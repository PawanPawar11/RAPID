using System;
using System.Collections.Generic;
using RAPID.Storage.Models;

namespace RAPID.Persistence;

public class KeySnapshotDto
{
    public string Key { get; set; } = string.Empty;
    public RedisDataType Type { get; set; }
    public string? StringValue { get; set; }
    public List<string>? ListValue { get; set; }
    public Dictionary<string, string>? HashValue { get; set; }
    public DateTime? ExpiresAtUtc { get; set; }
}

public class DatabaseSnapshotDto
{
    public DateTime SavedAtUtc { get; set; } = DateTime.UtcNow;
    public List<KeySnapshotDto> Keys { get; set; } = new();
}
