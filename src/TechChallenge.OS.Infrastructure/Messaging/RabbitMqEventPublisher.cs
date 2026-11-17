using System.Text.Json;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using TechChallenge.OS.Application.Interfaces.Services;

namespace TechChallenge.OS.Infrastructure.Messaging;

public sealed class RabbitMqEventPublisher : IOrdemServicoEventPublisher, IAsyncDisposable
{
    private readonly RabbitMqOptions _options;
    private readonly ILogger<RabbitMqEventPublisher> _logger;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private IConnection? _connection;

    public RabbitMqEventPublisher(RabbitMqOptions options, ILogger<RabbitMqEventPublisher> logger)
    {
        _options = options;
        _logger = logger;
    }

    public async Task PublicarOrdemServicoCriadaAsync(Guid ordemServicoUniqueCode, CancellationToken cancellationToken = default)
    {
        var evento = new OrdemServicoCriadaEvent(Guid.NewGuid(), ordemServicoUniqueCode, DateTime.UtcNow);
        var body = JsonSerializer.SerializeToUtf8Bytes(evento, JsonSerializerOptions.Web);

        var connection = await GetConnectionAsync(cancellationToken);
        await using var channel = await connection.CreateChannelAsync(
            new CreateChannelOptions(publisherConfirmationsEnabled: true, publisherConfirmationTrackingEnabled: true),
            cancellationToken);

        await channel.ExchangeDeclareAsync(RabbitMqOptions.ExchangeName, ExchangeType.Topic, durable: true, autoDelete: false, cancellationToken: cancellationToken);

        var props = new BasicProperties
        {
            ContentType = "application/json",
            DeliveryMode = DeliveryModes.Persistent,
            MessageId = evento.EventId.ToString()
        };

        await channel.BasicPublishAsync(RabbitMqOptions.ExchangeName, RabbitMqOptions.RkOsCriada, mandatory: false, basicProperties: props, body: body, cancellationToken: cancellationToken);
        _logger.LogInformation("Evento {RoutingKey} publicado. EventId={EventId} OS={OS}", RabbitMqOptions.RkOsCriada, evento.EventId, ordemServicoUniqueCode);
    }

    private async Task<IConnection> GetConnectionAsync(CancellationToken cancellationToken)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            if (_connection is not { IsOpen: true })
            {
                if (_connection is not null) await _connection.DisposeAsync();
                _connection = await _options.CreateConnectionFactory().CreateConnectionAsync(cancellationToken);
            }
            return _connection;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection is not null) await _connection.DisposeAsync();
        _lock.Dispose();
    }
}

public sealed class NullOrdemServicoEventPublisher : IOrdemServicoEventPublisher
{
    public Task PublicarOrdemServicoCriadaAsync(Guid ordemServicoUniqueCode, CancellationToken cancellationToken = default) => Task.CompletedTask;
}
