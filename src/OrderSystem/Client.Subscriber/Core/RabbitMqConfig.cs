using OrderSystem.RabbitMq.Contract.Models;

namespace OrderSystem.RabbitMq.Client.Publisher;


public class RabbitMqConfig
{
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string UserName { get; set; } = "orderuser";
    public string Password { get; set; } = "orderpass";
    public string QueueName { get; set; } = "orders";
    public ushort PrefetchCount { get; set; } = 100;
    public int AckTimeoutMs { get; set; } = 5000;

    public string ConnectionString =>
        $"amqp://{UserName}:{Password}@{Host}:{Port}/";

    public static RabbitMqConfig FromConnectionParameters(ConnectionParameters parameters)
    {
        return new RabbitMqConfig
        {
            Host = parameters.Host,
            Port = parameters.Port,
            UserName = parameters.UserName,
            Password = parameters.Password,
            QueueName = parameters.QueueName
        };
    }
}
