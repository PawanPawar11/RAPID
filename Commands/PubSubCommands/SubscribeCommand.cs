using System.Text;

namespace RAPID.Commands.PubSubCommands;

public class SubscribeCommand : ICommand
{
    public string Name => "SUBSCRIBE";

    public string Execute(CommandContext context, string[] parts)
    {
        if (parts.Length < 2)
        {
            return "-ERR wrong number of arguments for 'subscribe' command\r\n";
        }

        var sb = new StringBuilder();
        for (int i = 1; i < parts.Length; i++)
        {
            string channel = parts[i];
            int count = context.PubSub.Subscribe(channel, context.Session);

            sb.Append($"*3\r\n$9\r\nsubscribe\r\n${Encoding.UTF8.GetByteCount(channel)}\r\n{channel}\r\n:{count}\r\n");
        }

        return sb.ToString();
    }
}
