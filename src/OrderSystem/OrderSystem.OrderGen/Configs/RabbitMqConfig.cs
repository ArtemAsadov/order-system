namespace OrderSystem.OrderGen.Configs;

public class RabbitMqConfig
{
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string UserName { get; set; } = "orderuser";
    public string Password { get; set; } = "orderpass";
    public string QueueName { get; set; } = "orders";

    public string ConnectionString =>
        $"amqp://{UserName}:{Password}@{Host}:{Port}/";
}