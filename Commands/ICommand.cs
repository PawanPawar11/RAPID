using RAPID.Storage;

namespace RAPID.Commands;

public interface ICommand
{
    string Name { get; }
    string Execute(Database db, string[] parts);
}
