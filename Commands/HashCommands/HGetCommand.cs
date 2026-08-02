using System.Text;
using RAPID.Storage.Models;

namespace RAPID.Commands.HashCommands;

public class HGetCommand : ICommand
{
    public string Name => "HGET";

    public string Execute(CommandContext context, string[] parts)
    {
        if (parts.Length != 3)
        {
            return "-ERR wrong number of arguments for 'hget' command\r\n";
        }

        string key = parts[1];
        string field = parts[2];

        var result = context.Db.HGet(key, field);

        return result.Type switch
        {
            HashResultType.Success => $"${Encoding.UTF8.GetByteCount(result.Value!)}\r\n{result.Value}\r\n",
            HashResultType.KeyNotFound => "$-1\r\n",
            HashResultType.FieldNotFound => "$-1\r\n",
            HashResultType.WrongType => "-WRONGTYPE Operation against a key holding the wrong kind of value\r\n",
            _ => "-ERR unknown error\r\n"
        };
    }
}
