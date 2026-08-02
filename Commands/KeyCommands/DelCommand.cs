namespace RAPID.Commands.KeyCommands;

public class DelCommand : ICommand
{
    public string Name => "DEL";

    public string Execute(CommandContext context, string[] parts)
    {
        if (parts.Length < 2)
        {
            return "-ERR wrong number of arguments for 'del' command\r\n";
        }

        int deletedCount = 0;
        for (int i = 1; i < parts.Length; i++)
        {
            if (context.Db.Del(parts[i]))
            {
                deletedCount++;
            }
        }
        return $":{deletedCount}\r\n";
    }
}
