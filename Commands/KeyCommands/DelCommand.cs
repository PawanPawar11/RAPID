using RAPID.Storage;

namespace RAPID.Commands.KeyCommands;

public class DelCommand : ICommand
{
    public string Name => "DEL";

    public string Execute(Database db, string[] parts)
    {
        if (parts.Length < 2)
        {
            return "-ERR wrong number of arguments for 'del' command\r\n";
        }

        int deletedCount = 0;
        for (int i = 1; i < parts.Length; i++)
        {
            if (db.Del(parts[i]))
            {
                deletedCount++;
            }
        }
        return $":{deletedCount}\r\n";
    }
}
