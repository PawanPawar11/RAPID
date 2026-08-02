using System.Collections.Generic;

namespace RAPID.Storage.Models;

public enum GetResultType
{
    Success,
    NotFound,
    WrongType
}

public record GetResult(GetResultType Type, string? Value);

public enum NumericResultType
{
    Success,
    NotAnInteger,
    Overflow,
    WrongType
}

public record NumericResult(NumericResultType Type, long NewValue);

public enum ListResultType
{
    Success,
    WrongType,
    KeyNotFound
}

public record ListResult(ListResultType Type, long Length = 0, string? PopValue = null);

public enum HashResultType
{
    Success,
    WrongType,
    KeyNotFound,
    FieldNotFound
}

public record HashResult(
    HashResultType Type,
    long IntegerReply = 0,
    string? Value = null,
    Dictionary<string, string>? Entries = null);
