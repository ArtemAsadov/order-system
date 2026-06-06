namespace OrderSystem.RabbitMq.Contract.Abstractions;

public interface IMessagePublisher
{
    Task PublishAsync<T>(T message, CancellationToken ct = default) where T : IMessage;
    Task PublishBatchAsync<T>(IEnumerable<T> messages, CancellationToken ct = default) where T : IMessage;
}
