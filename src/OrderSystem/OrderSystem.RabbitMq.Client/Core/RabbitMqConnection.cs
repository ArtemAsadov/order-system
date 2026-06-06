using RabbitMQ.Client;

namespace OrderSystem.RabbitMq.Client.Publisher.Core;

public class RabbitMqConnection : IAsyncDisposable
{
    public IConnection Connection { get; }
    public IChannel Channel { get; }

    private RabbitMqConnection(IConnection connection, IChannel channel)
    {
        Connection = connection;
        Channel = channel;
    }

    public static async Task<RabbitMqConnection> CreateAsync(RabbitMqConfig config)
    {
        var factory = new ConnectionFactory
        {
            HostName = config.Host,
            Port = config.Port,
            UserName = config.UserName,
            Password = config.Password
        };

        var connection = await factory.CreateConnectionAsync();
        var channel = await connection.CreateChannelAsync();

        await channel.QueueDeclareAsync(
              queue: config.QueueName,           // "orders" - имя очереди
              durable: true,                     // переживёт рестарт
              exclusive: false,                  // доступна всем
              autoDelete: false,                 // не удалять автоматически
              arguments: null                    // без доп. настроек
        );

        return new RabbitMqConnection(connection, channel);
    }

    public async ValueTask DisposeAsync()
    {
        await Channel.CloseAsync();
        await Connection.CloseAsync();
    }
}
