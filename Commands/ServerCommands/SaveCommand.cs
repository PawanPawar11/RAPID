using System;
using RAPID.Persistence;

namespace RAPID.Commands.ServerCommands;

public class SaveCommand : ICommand
{
    private readonly PersistenceManager _persistenceManager;

    public SaveCommand(PersistenceManager persistenceManager)
    {
        _persistenceManager = persistenceManager;
    }

    public string Name => "SAVE";

    public string Execute(CommandContext context, string[] parts)
    {
        try
        {
            _persistenceManager.Save(context.Db);
            return "+OK\r\n";
        }
        catch (Exception ex)
        {
            return $"-ERR save failed: {ex.Message}\r\n";
        }
    }
}
