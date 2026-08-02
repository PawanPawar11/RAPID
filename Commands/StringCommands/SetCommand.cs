using RAPID.Storage;

namespace RAPID.Commands.StringCommands;

public class SetCommand : ICommand
{
    public string Name => "SET";

    public string Execute(Database db, string[] parts)
    {
        if (parts.Length < 3)
        {
            return "-ERR wrong number of arguments for 'set' command\r\n";
        }

        string key = parts[1];
        string value = string.Join(" ", parts, 2, parts.Length - 2);
        db.Set(key, value);

        return "+OK\r\n";
    }
}
