namespace RAPID.Commands.KeyCommands;

public class ExpireCommand : ICommand
{
    public string Name => "EXPIRE";

    public string Execute(CommandContext context, string[] parts)
    {
        if (parts.Length < 3 || !int.TryParse(parts[2], out int seconds))
        {
            return "-ERR wrong number of arguments or invalid seconds for 'expire' command\r\n";
        }

        string key = parts[1];
        int result = context.Db.Expire(key, seconds);
        return $":{result}\r\n";
    }
}
