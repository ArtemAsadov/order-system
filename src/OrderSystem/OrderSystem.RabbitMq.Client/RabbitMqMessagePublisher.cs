using OrderSystem.RabbitMq.Client.Publisher;
using OrderSystem.RabbitMq.Client.Publisher.Core;
using OrderSystem.RabbitMq.Contract.Abstractions;
using OrderSystem.RabbitMq.Contract.Models;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace Client.Publisher;

public class RabbitMqMessagePublisher : IMessagePublisher
{
    private readonly Lazy<Task<RabbitMqConnection>> _lazyConnection;
    private readonly string _queueName;

    public RabbitMqMessagePublisher(ConnectionParameters parameters)
    {
        var config = RabbitMqConfig.FromConnectionParameters(parameters);
        _queueName = config.QueueName;
        _lazyConnection = new Lazy<Task<RabbitMqConnection>>(
            () => RabbitMqConnection.CreateAsync(config)
        );
    }

    /// <summary>
    /// Отправить одно сообщение
    /// </summary>
    public async Task PublishAsync<T>(T message, CancellationToken cancellationToken = default) where T : IMessage
    {
        var connection = await _lazyConnection.Value;
        var json = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(json);

        await connection.Channel.BasicPublishAsync(
            exchange: string.Empty,
            routingKey: _queueName,
            body: body,
            cancellationToken: cancellationToken
        );
    }

    /// <summary>
    /// Отправить несколько сообщений (батчем)
    /// </summary>
    public async Task PublishBatchAsync<T>(IEnumerable<T> messages, CancellationToken cancellationToken = default) where T : IMessage
    {
        var connection = await _lazyConnection.Value;

        foreach (var message in messages)
        {
            var json = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(json);

            await connection.Channel.BasicPublishAsync(
                exchange: string.Empty,
                routingKey: _queueName,
                body: body,
                cancellationToken: cancellationToken
            );
        }
    }
}
