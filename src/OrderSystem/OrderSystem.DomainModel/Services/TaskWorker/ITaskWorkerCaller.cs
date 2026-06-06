using OrderSystem.TaskMgr.Proto;

namespace OrderSystem.DomainModel.Services.TaskWorker;

public interface ITaskWorkerCaller
{
    Task<TaskRequest> CreateRequestAsync(TaskAck? context = null, CancellationToken ct = default);
    Task OnAckReceivedAsync(TaskAck ack, CancellationToken ct = default);
}