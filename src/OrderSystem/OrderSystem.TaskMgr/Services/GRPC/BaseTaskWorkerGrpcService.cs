using Grpc.Core;
using OrderSystem.TaskMgr.Proto;
using OrderSystem.TaskMgr.Services.TaskWorker;

namespace OrderSystem.TaskWorker.Services;

public class BaseTaskWorkerGrpcService : TaskWorkerService.TaskWorkerServiceBase
{
    private readonly TaskWorkerStreamProcessor _processor;

    public BaseTaskWorkerGrpcService(TaskWorkerStreamProcessor processor)
    {
        _processor = processor;
    }

    public override async Task SendTasks(
        IAsyncStreamReader<TaskRequest> requestStream,
        IServerStreamWriter<TaskAck> responseStream,
        ServerCallContext context)
    {
        await _processor.HandleStreamAsync(requestStream, responseStream, context.CancellationToken);
    }
}