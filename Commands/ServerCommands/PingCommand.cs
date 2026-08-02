using RAPID.Storage;

namespace RAPID.Commands.ServerCommands;

public class PingCommand : ICommand
{
    public string Name => "PING";

    public string Execute(Database db, string[] parts)
    {
        return "+PONG\r\n";
    }
}
