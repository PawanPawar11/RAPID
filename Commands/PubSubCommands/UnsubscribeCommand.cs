using System.Text;

namespace RAPID.Commands.PubSubCommands;

public class UnsubscribeCommand : ICommand
{
    public string Name => "UNSUBSCRIBE";

    public string Execute(CommandContext context, string[] parts)
    {
        var sb = new StringBuilder();

        if (parts.Length == 1)
        {
            context.PubSub.UnsubscribeAll(context.Session, (channel, remainingCount) =>
            {
                sb.Append($"*3\r\n$11\r\nunsubscribe\r\n${Encoding.UTF8.GetByteCount(channel)}\r\n{channel}\r\n:{remainingCount}\r\n");
            });

            if (sb.Length == 0)
            {
                sb.Append("*3\r\n$11\r\nunsubscribe\r\n$-1\r\n:0\r\n");
            }
        }
        else
        {
            for (int i = 1; i < parts.Length; i++)
            {
                string channel = parts[i];
                int remainingCount = context.PubSub.Unsubscribe(channel, context.Session);

                sb.Append($"*3\r\n$11\r\nunsubscribe\r\n${Encoding.UTF8.GetByteCount(channel)}\r\n{channel}\r\n:{remainingCount}\r\n");
            }
        }

        return sb.ToString();
    }
}
