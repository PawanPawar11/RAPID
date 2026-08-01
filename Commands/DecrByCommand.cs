using RAPID.Storage;

namespace RAPID.Commands;

public static class DecrByCommand
{
    public static string Execute(Database db, string[] parts)
    {
        if (parts.Length != 3)
        {
            return "-ERR wrong number of arguments for 'decrby' command\r\n";
        }

        string key = parts[1];
        if (!long.TryParse(parts[2], out long decrement))
        {
            return "-ERR value is not an integer or out of range\r\n";
        }

        try
        {
            long amount = checked(-decrement);
            var result = db.IncrBy(key, amount);

            return result.Type switch
            {
                NumericResultType.Success => $":{result.NewValue}\r\n",
                NumericResultType.NotAnInteger => "-ERR value is not an integer or out of range\r\n",
                NumericResultType.Overflow => "-ERR increment or decrement would overflow\r\n",
                _ => "-ERR unknown error\r\n"
            };
        }
        catch (OverflowException)
        {
            return "-ERR increment or decrement would overflow\r\n";
        }
    }
}
