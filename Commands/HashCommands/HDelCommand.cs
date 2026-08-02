using RAPID.Storage;
using RAPID.Storage.Models;

namespace RAPID.Commands.HashCommands;

public class HDelCommand : ICommand
{
    public string Name => "HDEL";

    public string Execute(Database db, string[] parts)
    {
        if (parts.Length != 3)
        {
            return "-ERR wrong number of arguments for 'hdel' command\r\n";
        }

        string key = parts[1];
        string field = parts[2];

        var result = db.HDel(key, field);

        return result.Type switch
        {
            HashResultType.Success => $":{result.IntegerReply}\r\n",
            HashResultType.KeyNotFound => ":0\r\n",
            HashResultType.WrongType => "-WRONGTYPE Operation against a key holding the wrong kind of value\r\n",
            _ => "-ERR unknown error\r\n"
        };
    }
}
