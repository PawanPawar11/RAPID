namespace RAPID.Commands.KeyCommands;

public class TtlCommand : ICommand
{
    public string Name => "TTL";

    public string Execute(CommandContext context, string[] parts)
    {
        if (parts.Length != 2)
        {
            return "-ERR wrong number of arguments for 'ttl' command\r\n";
        }

        string key = parts[1];
        long ttl = context.Db.Ttl(key);
        return $":{ttl}\r\n";
    }
}
