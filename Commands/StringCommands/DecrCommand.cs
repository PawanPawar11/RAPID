using RAPID.Storage.Models;

namespace RAPID.Commands.StringCommands;

public class DecrCommand : ICommand
{
    public string Name => "DECR";

    public string Execute(CommandContext context, string[] parts)
    {
        if (parts.Length != 2)
        {
            return "-ERR wrong number of arguments for 'decr' command\r\n";
        }

        string key = parts[1];
        var result = context.Db.IncrBy(key, -1);

        return result.Type switch
        {
            NumericResultType.Success => $":{result.NewValue}\r\n",
            NumericResultType.NotAnInteger => "-ERR value is not an integer or out of range\r\n",
            NumericResultType.Overflow => "-ERR increment or decrement would overflow\r\n",
            NumericResultType.WrongType => "-WRONGTYPE Operation against a key holding the wrong kind of value\r\n",
            _ => "-ERR unknown error\r\n"
        };
    }
}
