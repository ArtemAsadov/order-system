using OrderSystem.RabbitMq.Client.Publisher;
using OrderSystem.RabbitMq.Client.Publisher.Core;
using OrderSystem.RabbitMq.Contract.Abstractions;
using OrderSystem.RabbitMq.Contract.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;

namespace Client.Subscriber;

public class RabbitMqMessageSubscriber : IMessageSubscriber, IAsyncDisposable
{
    private readonly Lazy<Task<RabbitMqConnection>> _lazyConnection;
    private readonly ConcurrentDictionary<string, Task> _subsctiptions = new();
    private bool _disposed;

    public RabbitMqMessageSubscriber(ConnectionParameters parametrs)
    {
        var config = RabbitMqConfig.FromConnectionParameters(parametrs);
        _lazyConnection = new(() => RabbitMqConnection.CreateAsync(config));
    }

    public async Task SubscibeAsync<T>(string queueName, Func<T, Task> handler, CancellationToken ct = default) where T : IMessage
    {
        if (_subsctiptions.ContainsKey(queueName))
            return;

        var connection = await _lazyConnection.Value;
        var consumer = new AsyncEventingBasicConsumer(connection.Channel);

        consumer.ReceivedAsync += async (_, args) =>
        {
            try
            {
                var json = Encoding.UTF8.GetString(args.Body.ToArray());
                var message = JsonSerializer.Deserialize<T>(json);

                if (message != null)
                {
                    await handler(message);
                }

                await connection.Channel.BasicAckAsync(args.DeliveryTag, false, ct);
            }
            catch
            {
                await connection.Channel.BasicNackAsync(args.DeliveryTag, false, true, ct);
            }
        };

        await connection.Channel.BasicConsumeAsync(queueName, false, consumer, ct);

        _subsctiptions.TryAdd(queueName, Task.CompletedTask);
    }

    public async Task<ChannelReader<T>> SubscribeAsChannelAsync<T>(string queueName, int capacity = 100, CancellationToken ct = default) where T : IMessage
    {
        var channel = Channel.CreateBounded<T>(capacity);
        var connection = await _lazyConnection.Value;
        var consumer = new AsyncEventingBasicConsumer(connection.Channel);

        consumer.ReceivedAsync += async (_, args) =>
        {
            try
            {
                var json = Encoding.UTF8.GetString(args.Body.ToArray());
                var message = JsonSerializer.Deserialize<T>(json);

                if (message != null)
                {
                    await channel.Writer.WriteAsync(message);
                }

                await connection.Channel.BasicAckAsync(args.DeliveryTag, false, ct);
            }
            catch
            {
                await connection.Channel.BasicNackAsync(args.DeliveryTag, false, true, ct);
            }
        };

        await connection.Channel.BasicConsumeAsync(
          queue: "orders",    // какую очередь слушаем
          autoAck: false,     // сами будем подтверждать получение
          consumer: consumer,  // наш слушатель
          ct);

        return channel.Reader;
    }

    public void Unsubscribe(string queueName)
    {
        _subsctiptions.TryRemove(queueName, out _);
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;

        _subsctiptions.Clear();

        if (_lazyConnection.IsValueCreated)
        {
            var connection = await _lazyConnection.Value;
            await connection.DisposeAsync();
        }

        _disposed = true;
    }
}
