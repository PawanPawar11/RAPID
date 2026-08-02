namespace RAPID.Commands.ServerCommands;

public class PingCommand : ICommand
{
    public string Name => "PING";

    public string Execute(CommandContext context, string[] parts)
    {
        return "+PONG\r\n";
    }
}
