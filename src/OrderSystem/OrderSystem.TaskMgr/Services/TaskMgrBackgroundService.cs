using Contract.Messages.Orders;
using OrderSystem.OrderGen.Configs;
using OrderSystem.RabbitMq.Contract.Abstractions;
using OrderSystem.RabbitMq.Contract.Models;
using OrderSystem.TaskMgr.Services.TaskWorker;

namespace OrderSystem.TaskMgr.Services;

public class TaskMgrBackgroundService : BackgroundService, IAsyncDisposable
{
    private readonly ILogger<TaskMgrBackgroundService> _logger;
    private readonly IMessageSubscriber _subscriber;
    private readonly IServiceProvider _serviceProvider;
    private readonly ConnectionParameters _connectionParams;
    private readonly TaskMngConfig _taskMngConfig;
    private CancellationTokenSource? _cts;
    private TaskWorkerStreamClient? _streamClient;  // ← добавили поле

    public TaskMgrBackgroundService(
        ILogger<TaskMgrBackgroundService> logger,
        IMessageSubscriber subscriber,
        IServiceProvider serviceProvider,
        ConnectionParameters connectionParams,
        TaskMngConfig taskMngConfig)
    {
        _logger = logger;
        _subscriber = subscriber;
        _serviceProvider = serviceProvider;
        _connectionParams = connectionParams;
        _taskMngConfig = taskMngConfig;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _cts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);

        _logger.LogInformation("TaskMgr Background Service started");

        try
        {
            using var scope = _serviceProvider.CreateScope();

            // Получаем клиент и подключаемся
            _streamClient = scope.ServiceProvider.GetRequiredService<TaskWorkerStreamClient>();
            await _streamClient.ConnectAsync(_taskMngConfig.WorkerGrpcAddress, _cts.Token);

            // Получаем caller и хендлер
            var caller = scope.ServiceProvider.GetRequiredService<OrderTaskCaller>();
            var orderHandler = scope.ServiceProvider.GetRequiredService<OrderCommandHandler>();

            // Подписываемся на RabbitMQ
            await _subscriber.SubscibeAsync<ProcessOrderCommand>(
                _taskMngConfig.QueueName ?? "tasks_queue",
                async (command) =>
                {
                    // Устанавливаем команду в caller
                    caller.SetPendingCommand(command);

                    // Создаем запрос через caller
                    var request = await caller.CreateRequestAsync(ct: _cts!.Token);

                    // Отправляем через клиент
                    await _streamClient.SendAsync(request, _cts.Token);

                    _logger.LogInformation("Order {OrderId} sent to worker", command.Order.OrderId);
                },
                _cts.Token);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in TaskMgr Background Service");
        }

        await Task.Delay(-1, _cts.Token);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("TaskMgr Background Service stopping...");
        _cts?.Cancel();

        if (_streamClient != null)
        {
            await _streamClient.DisposeAsync();
        }

        await base.StopAsync(cancellationToken);
    }

    public override void Dispose()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        base.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        _cts?.Cancel();
        _cts?.Dispose();

        if (_streamClient != null)
            await _streamClient.DisposeAsync();
    }
}