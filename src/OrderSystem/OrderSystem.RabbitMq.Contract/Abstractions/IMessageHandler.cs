namespace OrderSystem.RabbitMq.Contract.Abstractions
{
    public interface IMessageHandler<T> where T: IMessage
    {
        Task HandleAsync(T message, CancellationToken ct = default);
    }
}
