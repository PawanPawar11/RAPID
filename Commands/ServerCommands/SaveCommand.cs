using System;
using RAPID.Persistence;
using RAPID.Storage;

namespace RAPID.Commands.ServerCommands;

public class SaveCommand : ICommand
{
    private readonly PersistenceManager _persistenceManager;

    public SaveCommand(PersistenceManager persistenceManager)
    {
        _persistenceManager = persistenceManager;
    }

    public string Name => "SAVE";

    public string Execute(Database db, string[] parts)
    {
        try
        {
            _persistenceManager.Save(db);
            return "+OK\r\n";
        }
        catch (Exception ex)
        {
            return $"-ERR save failed: {ex.Message}\r\n";
        }
    }
}
