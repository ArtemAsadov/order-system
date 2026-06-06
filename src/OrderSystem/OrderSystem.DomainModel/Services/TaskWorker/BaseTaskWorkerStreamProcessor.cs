using Grpc.Core;
using Microsoft.Extensions.Logging;
using OrderSystem.DomainModel.Services.Base;
using OrderSystem.TaskMgr.Proto;

namespace OrderSystem.DomainModel.Services.TaskWorker;

/// <summary>
/// Базовый процессор для TaskWorker (без привязки к конкретным командам)
/// </summary>
public abstract class BaseTaskWorkerStreamProcessor : BaseStreamProcessor<TaskRequest, TaskAck>
{
    private readonly ITaskWorkerHandler _handler;

    protected BaseTaskWorkerStreamProcessor(ITaskWorkerHandler handler, ILogger logger)
        : base(logger)
    {
        _handler = handler;
    }

    protected sealed override Task<TaskAck> ProcessAsync(TaskRequest request, CancellationToken ct)
    {
        return _handler.HandleTaskAsync(request, ct);
    }

    // Абстрактный метод для обработки конкретных типов (реализуется в наследнике)
    protected abstract Task<TaskAck> ProcessCommandAsync(TaskRequest request, CancellationToken ct);
}