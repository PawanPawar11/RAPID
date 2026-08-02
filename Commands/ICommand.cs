namespace RAPID.Commands;

public interface ICommand
{
    string Name { get; }
    string Execute(CommandContext context, string[] parts);
}
