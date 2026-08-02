using RAPID.Storage;
using RAPID.Storage.Models;

namespace RAPID.Commands.ListCommands;

public class LLenCommand : ICommand
{
    public string Name => "LLEN";

    public string Execute(Database db, string[] parts)
    {
        if (parts.Length != 2)
        {
            return "-ERR wrong number of arguments for 'llen' command\r\n";
        }

        string key = parts[1];
        var result = db.LLen(key);

        return result.Type switch
        {
            ListResultType.Success => $":{result.Length}\r\n",
            ListResultType.WrongType => "-WRONGTYPE Operation against a key holding the wrong kind of value\r\n",
            _ => "-ERR unknown error\r\n"
        };
    }
}
