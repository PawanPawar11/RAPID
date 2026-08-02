using RAPID.Persistence;

namespace RAPID.Commands.ServerCommands;

public class BgSaveCommand : ICommand
{
    private readonly PersistenceManager _persistenceManager;

    public BgSaveCommand(PersistenceManager persistenceManager)
    {
        _persistenceManager = persistenceManager;
    }

    public string Name => "BGSAVE";

    public string Execute(CommandContext context, string[] parts)
    {
        if (_persistenceManager.SaveBackground(context.Db))
        {
            return "+Background saving started\r\n";
        }

        return "-ERR Background save already in progress\r\n";
    }
}
