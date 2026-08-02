using System.Text;
using RAPID.Storage;
using RAPID.Storage.Models;

namespace RAPID.Commands.ListCommands;

public class RPopCommand : ICommand
{
    public string Name => "RPOP";

    public string Execute(Database db, string[] parts)
    {
        if (parts.Length != 2)
        {
            return "-ERR wrong number of arguments for 'rpop' command\r\n";
        }

        string key = parts[1];
        var result = db.RPop(key);

        return result.Type switch
        {
            ListResultType.Success => $"${Encoding.UTF8.GetByteCount(result.PopValue!)}\r\n{result.PopValue}\r\n",
            ListResultType.KeyNotFound => "$-1\r\n",
            ListResultType.WrongType => "-WRONGTYPE Operation against a key holding the wrong kind of value\r\n",
            _ => "-ERR unknown error\r\n"
        };
    }
}
