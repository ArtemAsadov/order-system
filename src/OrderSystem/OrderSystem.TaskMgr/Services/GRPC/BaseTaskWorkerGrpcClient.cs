using Grpc.Core;
using Grpc.Net.Client;
using OrderSystem.DomainModel.Services.Base;
using OrderSystem.TaskMgr.Proto;

namespace OrderSystem.DomainModel.Services.TaskWorker;

/// <summary>
/// Базовый gRPC клиент для TaskWorker
/// </summary>
public abstract class BaseTaskWorkerGrpcClient : BaseStreamClient<TaskRequest, TaskAck>
{
    private readonly string _address;

    protected BaseTaskWorkerGrpcClient(ILogger logger, string address) : base(logger)
    {
        _address = address;
    }

    protected sealed override async Task<AsyncDuplexStreamingCall<TaskRequest, TaskAck>> CreateStreamAsync(
        GrpcChannel channel,
        CancellationToken ct)
    {
        var client = new TaskWorkerService.TaskWorkerServiceClient(channel);
        return client.SendTasks(cancellationToken: ct);
    }

    protected sealed override Task<TaskRequest> CreateRequestAsync(TaskAck? context = null)
    {
        return CreateTaskRequestAsync(context);
    }

    protected abstract Task<TaskRequest> CreateTaskRequestAsync(TaskAck? context = null);

    public async Task ConnectAsync(CancellationToken ct = default)
    {
        await ConnectAsync(_address, ct);
    }
}