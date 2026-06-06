using Microsoft.Extensions.Options;
using OrderSystem.OrderGen.Configs;
using OrderSystem.OrderGen.Models;
using RabbitMQ.Client.Exceptions;

namespace OrderSystem.OrderGen.Services;


public sealed class OrderGeneratorService : IDisposable
{
    private readonly OrderPublisher _orderPublisher;
    private readonly GeneratorConfig _config;
    private int _isRunning = 0;
    private CancellationTokenSource? _cts;
    private long _totalOrdersSent = 0;
    private readonly Random _random = new();
    private volatile bool _disposed = false;
    private readonly ILogger<OrderGeneratorService> _logger;

    public OrderGeneratorService(OrderPublisher publisher, GeneratorConfig config, ILogger<OrderGeneratorService> logger)
    {
        _orderPublisher = publisher;
        _config = config;
        _logger = logger;
    }

    public bool IsRunning => _isRunning == 1;
    public long TotalOrdersSent => Interlocked.Read(ref _totalOrdersSent);

    public void Start()
    {
        if (_disposed) throw new ObjectDisposedException(nameof(OrderGeneratorService));
        if (Interlocked.Exchange(ref _isRunning, 1) == 1) return;

        _cts = new CancellationTokenSource();
        _ = Task.Run(() => RunAsync(_cts.Token));
    }

    public void Stop()
    {
        if (_disposed) return;
        if (Interlocked.Exchange(ref _isRunning, 0) == 0) return;

        _cts?.Cancel();
    }

    private async Task RunAsync(CancellationToken token)
    {
        var delayMs = 1000.0 / _config.DefaultRps;
        delayMs = Math.Max(1, delayMs);

        long orderId = 0;

        while (!token.IsCancellationRequested && !_disposed)
        {
            orderId++;

            try
            {
                var order = ToOrder(orderId);
                order.CalculateTotal();

                await _orderPublisher.PublishOrderAsync(order);
                Interlocked.Increment(ref _totalOrdersSent);

                if (orderId % 1000 == 0)
                {
                    Console.WriteLine($"📊 Sent {orderId} orders (RPS: {_config.DefaultRps})");
                }
            }
            catch (BrokerUnreachableException ex)
            {
                _logger?.LogError(ex, "RabbitMQ is down! Stopping generator.");
                Stop(); // Останавливаем генератор
                break;  // Выходим из цикла
            }
            catch (AlreadyClosedException ex)
            {
                _logger?.LogError(ex, "RabbitMQ channel already closed. Stopping generator.");
                Stop();
                break;
            }
            catch (OperationInterruptedException ex)
            {
                _logger?.LogError(ex, "RabbitMQ connection interrupted. Stopping generator.");
                Stop();
                break;
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger?.LogCritical(ex, "RabbitMQ auth failed! Stopping generator.");
                Stop();
                break;
            }
            catch (TimeoutException ex)
            {
                _logger?.LogError(ex, "Publish timeout, retrying...");
                orderId--; // откатываем, пробуем снова
                await Task.Delay(1000, token);
                continue;
            }
            catch (IOException ex)
            {
                _logger?.LogError(ex, "Network error, retrying...");
                orderId--;
                await Task.Delay(1000, token);
                continue;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Unexpected error, retrying...");
                orderId--;
                await Task.Delay(1000, token);
                continue;
            }

            await Task.Delay((int)delayMs, token);
        }
    }

    private Order ToOrder(long orderId)
    {
        var order = new Order
        {
            OrderId = orderId,
            UserId = _random.Next(1, _config.MaxUsers + 1),
            Category = _config.Categories[_random.Next(_config.Categories.Length)],
            Amount = _random.Next(1, 11),
            UnitPrice = Math.Round(_random.Next(100, 10000) / 100m, 2),
            CreatedAt = DateTime.UtcNow
        };
        order.CalculateTotal();
        return order;
    }

    public void Dispose()
    {
        if (_disposed) return;

        Stop();
        _cts?.Dispose();

        _disposed = true;
    }
}