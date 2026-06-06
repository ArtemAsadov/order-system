using OrderSystem.RabbitMq.Contract.Abstractions;
using OrderSystem.TaskMgr.Services.TaskWorker;

public class OrderCommandHandler : IMessageHandler<Contract.Messages.Orders.ProcessOrderCommand>
{
    private readonly TaskWorkerStreamClient _streamClient;

    public OrderCommandHandler(TaskWorkerStreamClient streamClient)
    {
        _streamClient = streamClient;
    }

    public async Task HandleAsync(Contract.Messages.Orders.ProcessOrderCommand message, CancellationToken ct)
    {
        await _streamClient.SendOrderAsync(message, ct);
    }
}