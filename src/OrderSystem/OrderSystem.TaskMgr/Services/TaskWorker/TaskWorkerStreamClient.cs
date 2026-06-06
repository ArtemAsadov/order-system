using OrderSystem.DomainModel.Services.TaskWorker;
using OrderSystem.TaskMgr.Proto;

namespace OrderSystem.TaskMgr.Services.TaskWorker;

public class TaskWorkerStreamClient : BaseTaskWorkerStreamClient
{
    public TaskWorkerStreamClient(ITaskWorkerCaller caller, ILogger<TaskWorkerStreamClient> logger)
        : base(caller, logger)
    {
    }

    // Конкретная реализация для ProcessOrderCommand
    public async Task SendOrderAsync(Contract.Messages.Orders.ProcessOrderCommand command, CancellationToken ct = default)
    {
        var processOrderTask = new ProcessOrderTask
        {
            OrderId = command.Order.OrderId,
            CustomerId = command.Order.UserId,
            TotalAmount = (double)command.Order.TotalPrice,
            Category = command.Order.Category,
            Amount = command.Order.Amount,
            UnitPrice = (double)command.Order.UnitPrice
        };

        var request = new TaskRequest
        {
            TaskId = command.Order.OrderId.ToString(),
            TaskType = "ProcessOrder",
            ProcessOrder = processOrderTask,
            CreatedAt = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.UtcNow)
        };

        await SendAsync(request, ct);
    }
}