namespace OrderSystem.DomainModel.Commands;

public interface ICommand
{
    string CommandId { get; }
    DateTime CreatedAt { get; }
}
