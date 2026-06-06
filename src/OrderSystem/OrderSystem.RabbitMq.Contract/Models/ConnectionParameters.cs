namespace OrderSystem.RabbitMq.Contract.Models;

public class ConnectionParameters
{
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string UserName { get; set; } = "orderuser";
    public string Password { get; set; } = "orderpass";
    public string QueueName { get; set; } = "";
}
