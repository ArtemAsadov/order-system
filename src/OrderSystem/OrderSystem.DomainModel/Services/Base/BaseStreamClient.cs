using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;

namespace OrderSystem.DomainModel.Services.Base;

/// <summary>
/// Базовый клиент для duplex стрима
/// </summary>
public abstract class BaseStreamClient<TReq, TResp>
    where TReq : class
    where TResp : class
{
    private readonly ILogger _logger;
    private GrpcChannel? _channel;
    private AsyncDuplexStreamingCall<TReq, TResp>? _call;
    private bool _connected;

    protected BaseStreamClient(ILogger logger)
    {
        _logger = logger;
    }

    // Абстрактные методы для наследников
    protected abstract Task<AsyncDuplexStreamingCall<TReq, TResp>> CreateStreamAsync(
        GrpcChannel channel,
        CancellationToken ct);

    protected abstract Task<TReq> CreateRequestAsync(TResp? context = null);
    protected abstract Task OnResponseAsync(TResp response, CancellationToken ct);

    // Публичный метод для подключения
    public async Task ConnectAsync(string address, CancellationToken ct = default)
    {
        _channel = GrpcChannel.ForAddress(address);
        _call = await CreateStreamAsync(_channel, ct);
        _connected = true;

        _ = Task.Run(() => ProcessResponsesAsync(_call, ct), ct);

        _logger.LogInformation("Connected to {Address}", address);
    }

    // Публичный метод для отправки
    public async Task SendAsync(TReq request, CancellationToken ct = default)
    {
        if (!_connected) throw new InvalidOperationException("Not connected");
        await _call!.RequestStream.WriteAsync(request, ct);
        _logger.LogDebug("Request sent");
    }

    // Приватный метод обработки ответов
    private async Task ProcessResponsesAsync(AsyncDuplexStreamingCall<TReq, TResp> call, CancellationToken ct)
    {
        await foreach (var response in call.ResponseStream.ReadAllAsync(ct))
        {
            await OnResponseAsync(response, ct);
        }
    }

    // Публичный метод завершения
    public async Task CompleteAsync()
    {
        if (_call != null)
        {
            await _call.RequestStream.CompleteAsync();
        }
    }

    public async ValueTask DisposeAsync()
    {
        await CompleteAsync();
        if (_channel != null) await _channel.ShutdownAsync();
    }
}