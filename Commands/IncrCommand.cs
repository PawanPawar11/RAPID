using RAPID.Storage;

namespace RAPID.Commands;

public static class IncrCommand
{
    public static string Execute(Database db, string[] parts)
    {
        if (parts.Length != 2)
        {
            return "-ERR wrong number of arguments for 'incr' command\r\n";
        }

        string key = parts[1];
        var result = db.IncrBy(key, 1);

        return result.Type switch
        {
            NumericResultType.Success => $":{result.NewValue}\r\n",
            NumericResultType.NotAnInteger => "-ERR value is not an integer or out of range\r\n",
            NumericResultType.Overflow => "-ERR increment or decrement would overflow\r\n",
            _ => "-ERR unknown error\r\n"
        };
    }
}
