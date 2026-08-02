namespace RAPID.Commands.PubSubCommands;

public class PublishCommand : ICommand
{
    public string Name => "PUBLISH";

    public string Execute(CommandContext context, string[] parts)
    {
        if (parts.Length < 3)
        {
            return "-ERR wrong number of arguments for 'publish' command\r\n";
        }

        string channel = parts[1];
        string message = string.Join(" ", parts, 2, parts.Length - 2);

        int receiversCount = context.PubSub.Publish(channel, message);

        return $":{receiversCount}\r\n";
    }
}
