using OrderSystem.OrderGen.Models;
using OrderSystem.RabbitMq.Contract.Abstractions;

namespace OrderSystem.OrderGen.Commands
{
    public class OrderPlacedCommand : Message
    {
        public Order Order { get; set; }

        public OrderPlacedCommand(Order order)
        {
            Order = order;
        }
    }
}
