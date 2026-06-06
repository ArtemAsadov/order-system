using OrderSystem.OrderGen.Models;
using OrderSystem.RabbitMq.Contract.Abstractions;

namespace Contract.Messages.Orders;

/// <summary>
/// Команда на обработку заказа (аналог вашего OrderPlacedCommand)
/// </summary>
public class ProcessOrderCommand : Message
{
    public Order Order { get; set; }

    public ProcessOrderCommand(Order order)
    {
        Order = order;
    }
}