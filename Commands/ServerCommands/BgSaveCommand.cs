using RAPID.Persistence;
using RAPID.Storage;

namespace RAPID.Commands.ServerCommands;

public class BgSaveCommand : ICommand
{
    private readonly PersistenceManager _persistenceManager;

    public BgSaveCommand(PersistenceManager persistenceManager)
    {
        _persistenceManager = persistenceManager;
    }

    public string Name => "BGSAVE";

    public string Execute(Database db, string[] parts)
    {
        if (_persistenceManager.SaveBackground(db))
        {
            return "+Background saving started\r\n";
        }

        return "-ERR Background save already in progress\r\n";
    }
}
