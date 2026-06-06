using OrderSystem.OrderGen.Commands;
using OrderSystem.OrderGen.Models;
using OrderSystem.RabbitMq.Contract.Abstractions;

namespace OrderSystem.OrderGen.Services;

public class OrderPublisher
{
    private readonly IMessagePublisher _publisher;

    public OrderPublisher(IMessagePublisher publisher)
    {
        _publisher = publisher;
    }

    public async Task PublishOrderAsync(Order order, CancellationToken ct = default)
    {
        var command = new OrderPlacedCommand(order);
        await _publisher.PublishAsync(command, ct);
    }

    public async Task PublishBatchAsync(IEnumerable<Order> orders, CancellationToken ct = default)
    {
        var commands = orders.Select(o => new OrderPlacedCommand(o));
        await _publisher.PublishBatchAsync(commands, ct);
    }
}
