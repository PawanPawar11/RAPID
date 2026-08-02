using System.Text;
using RAPID.Storage.Models;

namespace RAPID.Commands.HashCommands;

public class HGetAllCommand : ICommand
{
    public string Name => "HGETALL";

    public string Execute(CommandContext context, string[] parts)
    {
        if (parts.Length != 2)
        {
            return "-ERR wrong number of arguments for 'hgetall' command\r\n";
        }

        string key = parts[1];
        var result = context.Db.HGetAll(key);

        if (result.Type == HashResultType.WrongType)
        {
            return "-WRONGTYPE Operation against a key holding the wrong kind of value\r\n";
        }

        var entries = result.Entries;
        if (entries == null || entries.Count == 0)
        {
            return "*0\r\n";
        }

        var sb = new StringBuilder();
        sb.Append($"*{entries.Count * 2}\r\n");

        foreach (var kvp in entries)
        {
            sb.Append($"${Encoding.UTF8.GetByteCount(kvp.Key)}\r\n{kvp.Key}\r\n");
            sb.Append($"${Encoding.UTF8.GetByteCount(kvp.Value)}\r\n{kvp.Value}\r\n");
        }

        return sb.ToString();
    }
}
