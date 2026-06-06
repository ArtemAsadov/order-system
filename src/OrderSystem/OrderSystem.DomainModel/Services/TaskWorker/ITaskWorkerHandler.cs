using OrderSystem.TaskMgr.Proto;

namespace OrderSystem.DomainModel.Services.TaskWorker;

public interface ITaskWorkerHandler
{
    Task<TaskAck> HandleTaskAsync(TaskRequest request, CancellationToken ct);
}
