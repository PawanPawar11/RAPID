using System.Text;
using RAPID.Storage.Models;

namespace RAPID.Commands.StringCommands;

public class GetCommand : ICommand
{
    public string Name => "GET";

    public string Execute(CommandContext context, string[] parts)
    {
        if (parts.Length != 2)
        {
            return "-ERR wrong number of arguments for 'get' command\r\n";
        }

        string key = parts[1];
        var result = context.Db.Get(key);

        return result.Type switch
        {
            GetResultType.Success => $"${Encoding.UTF8.GetByteCount(result.Value!)}\r\n{result.Value}\r\n",
            GetResultType.NotFound => "$-1\r\n",
            GetResultType.WrongType => "-WRONGTYPE Operation against a key holding the wrong kind of value\r\n",
            _ => "-ERR unknown error\r\n"
        };
    }
}
