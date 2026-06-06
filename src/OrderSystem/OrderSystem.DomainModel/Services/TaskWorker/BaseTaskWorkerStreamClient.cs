using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using OrderSystem.DomainModel.Services.Base;
using OrderSystem.TaskMgr.Proto;

namespace OrderSystem.DomainModel.Services.TaskWorker;

public abstract class BaseTaskWorkerStreamClient : BaseStreamClient<TaskRequest, TaskAck>
{
    private readonly ITaskWorkerCaller _caller;

    protected BaseTaskWorkerStreamClient(ITaskWorkerCaller caller, ILogger logger)
        : base(logger)
    {
        _caller = caller;
    }

    protected sealed override async Task<AsyncDuplexStreamingCall<TaskRequest, TaskAck>> CreateStreamAsync(
        GrpcChannel channel,
        CancellationToken ct)
    {
        var client = new TaskWorkerService.TaskWorkerServiceClient(channel);
        return client.SendTasks(cancellationToken: ct);
    }

    protected sealed override async Task<TaskRequest> CreateRequestAsync(TaskAck? context = null)
    {
        return await _caller.CreateRequestAsync(context);
    }

    protected sealed override Task OnResponseAsync(TaskAck response, CancellationToken ct)
    {
        return _caller.OnAckReceivedAsync(response, ct);
    }

    // Внутренний метод отправки
    private async Task SendInternalAsync(TaskRequest request, CancellationToken ct = default)
    {
        await SendAsync(request, ct);
    }

    // Публичный метод для отправки
    public async Task SendCommandAsync(TaskRequest request, CancellationToken ct = default)
    {
        await SendInternalAsync(request, ct);
    }
}