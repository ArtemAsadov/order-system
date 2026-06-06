using Contract.Messages.Orders;
using OrderSystem.DomainModel.Services.TaskWorker;
using OrderSystem.TaskMgr.Proto;

public class OrderTaskCaller : ITaskWorkerCaller
{
    private readonly ILogger<OrderTaskCaller> _logger;
    private ProcessOrderCommand? _pendingCommand;

    public OrderTaskCaller(ILogger<OrderTaskCaller> logger)
    {
        _logger = logger;
    }

    public void SetPendingCommand(ProcessOrderCommand command)
    {
        _pendingCommand = command;
    }
    //TODO context ретрай логика
    public async Task<TaskRequest> CreateRequestAsync(TaskAck? context = null, CancellationToken ct = default)
    {
        if (_pendingCommand == null)
        {
            throw new InvalidOperationException("No pending command. Call SetPendingCommand first.");
        }

        var command = _pendingCommand;
        _pendingCommand = null;

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

        return await Task.FromResult(request);
    }

    public Task OnAckReceivedAsync(TaskAck ack, CancellationToken ct = default)
    {
        if (ack.Success)
        {
            _logger.LogInformation("Order {OrderId} processed successfully", ack.TaskId);
        }
        else
        {
            _logger.LogError("Order {OrderId} failed: {Error}", ack.TaskId, ack.Error);
        }

        return Task.CompletedTask;
    }
}