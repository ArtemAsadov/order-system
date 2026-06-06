namespace OrderSystem.OrderGen.Configs;

public class TaskMngConfig
{
    public string WorkerGrpcAddress { get; set; } = "http://localhost:50051";
    public string QueueName { get; set; } = "tasks_queue";  // ← добавили
    public int MaxConcurrentTasks { get; set; } = 10;
}