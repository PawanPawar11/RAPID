using System.Text;
using RAPID.Storage;

namespace RAPID.Commands;

public static class LPopCommand
{
    public static string Execute(Database db, string[] parts)
    {
        if (parts.Length != 2)
        {
            return "-ERR wrong number of arguments for 'lpop' command\r\n";
        }

        string key = parts[1];
        var result = db.LPop(key);

        return result.Type switch
        {
            ListResultType.Success => $"${Encoding.UTF8.GetByteCount(result.PopValue!)}\r\n{result.PopValue}\r\n",
            ListResultType.KeyNotFound => "$-1\r\n",
            ListResultType.WrongType => "-WRONGTYPE Operation against a key holding the wrong kind of value\r\n",
            _ => "-ERR unknown error\r\n"
        };
    }
}
