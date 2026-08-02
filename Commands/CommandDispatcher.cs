using System;
using System.Collections.Concurrent;
using RAPID.Commands.HashCommands;
using RAPID.Commands.KeyCommands;
using RAPID.Commands.ListCommands;
using RAPID.Commands.ServerCommands;
using RAPID.Commands.StringCommands;
using RAPID.Persistence;
using RAPID.Storage;

namespace RAPID.Commands;

public class CommandDispatcher
{
    private readonly ConcurrentDictionary<string, ICommand> _commands = new(StringComparer.OrdinalIgnoreCase);

    public CommandDispatcher(PersistenceManager persistenceManager)
    {
        RegisterCommand(new GetCommand());
        RegisterCommand(new SetCommand());
        RegisterCommand(new IncrCommand());
        RegisterCommand(new DecrCommand());
        RegisterCommand(new IncrByCommand());
        RegisterCommand(new DecrByCommand());

        RegisterCommand(new LPushCommand());
        RegisterCommand(new RPushCommand());
        RegisterCommand(new LPopCommand());
        RegisterCommand(new RPopCommand());
        RegisterCommand(new LLenCommand());

        RegisterCommand(new HSetCommand());
        RegisterCommand(new HGetCommand());
        RegisterCommand(new HDelCommand());
        RegisterCommand(new HExistsCommand());
        RegisterCommand(new HGetAllCommand());

        RegisterCommand(new DelCommand());
        RegisterCommand(new ExistsCommand());
        RegisterCommand(new ExpireCommand());
        RegisterCommand(new TtlCommand());

        RegisterCommand(new PingCommand());
        RegisterCommand(new SaveCommand(persistenceManager));
        RegisterCommand(new BgSaveCommand(persistenceManager));
    }

    public void RegisterCommand(ICommand command)
    {
        _commands[command.Name] = command;
    }

    public string Dispatch(Database db, string rawInput)
    {
        if (string.IsNullOrWhiteSpace(rawInput))
            return string.Empty;

        string[] parts = rawInput.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
            return string.Empty;

        string commandName = parts[0];

        if (_commands.TryGetValue(commandName, out var command))
        {
            return command.Execute(db, parts);
        }

        return $"-ERR unknown command '{commandName}'\r\n";
    }
}
