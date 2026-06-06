using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace OrderSystem.DomainModel.Services.Base;

/// <summary>
/// Базовая обработка потока запросов
/// </summary>
public abstract class BaseStreamProcessor<TReq, TResp>
    where TReq : class
    where TResp : class
{
    private readonly ILogger _logger;

    protected BaseStreamProcessor(ILogger logger)
    {
        _logger = logger;
    }

    protected abstract Task<TResp> ProcessAsync(TReq request, CancellationToken ct);

    public async Task HandleStreamAsync(
        IAsyncStreamReader<TReq> requestStream,
        IServerStreamWriter<TResp> responseStream,
        CancellationToken ct)
    {
        await foreach (var request in requestStream.ReadAllAsync(ct))
        {
            _logger.LogDebug("Request received");
            var response = await ProcessAsync(request, ct);
            await responseStream.WriteAsync(response);
        }
    }
}