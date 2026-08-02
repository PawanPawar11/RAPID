using RAPID.Storage;

namespace RAPID.Commands.KeyCommands;

public class ExistsCommand : ICommand
{
    public string Name => "EXISTS";

    public string Execute(Database db, string[] parts)
    {
        if (parts.Length < 2)
        {
            return "-ERR wrong number of arguments for 'exists' command\r\n";
        }

        int existingCount = 0;
        for (int i = 1; i < parts.Length; i++)
        {
            if (db.Exists(parts[i]))
            {
                existingCount++;
            }
        }
        return $":{existingCount}\r\n";
    }
}
