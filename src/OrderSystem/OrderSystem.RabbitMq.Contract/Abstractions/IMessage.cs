namespace OrderSystem.RabbitMq.Contract.Abstractions
{
    public interface IMessage
    {
        string MessageId { get; }
        string MessageType { get; }
        DateTime CreatedAt { get; }
    }

    public abstract class Message : IMessage
    {
        public string MessageId { get; set; } = Guid.NewGuid().ToString();
        public string MessageType => GetType().Name;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
