using System.Linq;
using RAPID.Storage.Models;

namespace RAPID.Commands.ListCommands;

public class RPushCommand : ICommand
{
    public string Name => "RPUSH";

    public string Execute(CommandContext context, string[] parts)
    {
        if (parts.Length < 3)
        {
            return "-ERR wrong number of arguments for 'rpush' command\r\n";
        }

        string key = parts[1];
        var values = parts.Skip(2);
        var result = context.Db.RPush(key, values);

        return result.Type switch
        {
            ListResultType.Success => $":{result.Length}\r\n",
            ListResultType.WrongType => "-WRONGTYPE Operation against a key holding the wrong kind of value\r\n",
            _ => "-ERR unknown error\r\n"
        };
    }
}
