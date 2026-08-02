using System.Linq;
using RAPID.Storage;

namespace RAPID.Commands;

public static class LPushCommand
{
    public static string Execute(Database db, string[] parts)
    {
        if (parts.Length < 3)
        {
            return "-ERR wrong number of arguments for 'lpush' command\r\n";
        }

        string key = parts[1];
        var values = parts.Skip(2);
        var result = db.LPush(key, values);

        return result.Type switch
        {
            ListResultType.Success => $":{result.Length}\r\n",
            ListResultType.WrongType => "-WRONGTYPE Operation against a key holding the wrong kind of value\r\n",
            _ => "-ERR unknown error\r\n"
        };
    }
}
