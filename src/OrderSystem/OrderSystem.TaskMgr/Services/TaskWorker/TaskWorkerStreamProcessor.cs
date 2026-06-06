using OrderSystem.DomainModel.Commands;
using OrderSystem.DomainModel.Services.TaskWorker;
using OrderSystem.OrderGen.Models;
using OrderSystem.TaskMgr.Proto;

namespace OrderSystem.TaskMgr.Services.TaskWorker;

public class TaskWorkerStreamProcessor : BaseTaskWorkerStreamProcessor
{
    private readonly ICommandHandler<ProcessOrderTask, ProcessOrderResult> _orderHandler;
   
    public TaskWorkerStreamProcessor(
        ITaskWorkerHandler handler,
        ICommandHandler<ProcessOrderTask, ProcessOrderResult> orderHandler,
        ILogger<TaskWorkerStreamProcessor> logger)
        : base(handler, logger)
    {
        _orderHandler = orderHandler;
    }

    protected override async Task<TaskAck> ProcessCommandAsync(TaskRequest request, CancellationToken ct)
    {
        // Здесь маршрутизация по типам задач
        return request.PayloadCase switch
        {
            TaskRequest.PayloadOneofCase.ProcessOrder => await ProcessOrderAsync(request, ct),
            _ => new TaskAck { TaskId = request.TaskId, Success = false, Error = "Unknown task type" }
        };
    }

    private async Task<TaskAck> ProcessOrderAsync(TaskRequest request, CancellationToken ct)
    {
        var result = await _orderHandler.HandleAsync(request.ProcessOrder, ct);

        return new TaskAck
        {
            TaskId = request.TaskId,
            Success = true,
            OrderResult = result
        };
    }
}