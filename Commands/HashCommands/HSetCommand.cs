using RAPID.Storage.Models;

namespace RAPID.Commands.HashCommands;

public class HSetCommand : ICommand
{
    public string Name => "HSET";

    public string Execute(CommandContext context, string[] parts)
    {
        if (parts.Length < 4)
        {
            return "-ERR wrong number of arguments for 'hset' command\r\n";
        }

        string key = parts[1];
        string field = parts[2];
        string value = string.Join(" ", parts, 3, parts.Length - 3);

        var result = context.Db.HSet(key, field, value);

        return result.Type switch
        {
            HashResultType.Success => $":{result.IntegerReply}\r\n",
            HashResultType.WrongType => "-WRONGTYPE Operation against a key holding the wrong kind of value\r\n",
            _ => "-ERR unknown error\r\n"
        };
    }
}
