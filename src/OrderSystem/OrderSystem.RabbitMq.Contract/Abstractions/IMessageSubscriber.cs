using System.Threading.Channels;

namespace OrderSystem.RabbitMq.Contract.Abstractions;

public interface IMessageSubscriber
{
    Task SubscibeAsync<T>(string queueName, Func<T, Task> handler, CancellationToken ct = default) where T : IMessage;
    Task<ChannelReader<T>> SubscribeAsChannelAsync<T>(string queueName, int capacity = 100, CancellationToken ct = default) where T : IMessage;
}
