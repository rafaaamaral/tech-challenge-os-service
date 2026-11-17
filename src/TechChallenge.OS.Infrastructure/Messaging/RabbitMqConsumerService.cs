using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace TechChallenge.OS.Infrastructure.Messaging;

public sealed class RabbitMqConsumerService : BackgroundService
{
    private const int MaxAttempts = 3;

    private readonly RabbitMqOptions _options;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<RabbitMqConsumerService> _logger;

    public RabbitMqConsumerService(RabbitMqOptions options, IServiceScopeFactory scopeFactory, ILogger<RabbitMqConsumerService> logger)
    {
        _options = options;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Consumer RabbitMQ indisponível. Nova tentativa em 5s.");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }

    private async Task RunAsync(CancellationToken ct)
    {
        await using var connection = await _options.CreateConnectionFactory().CreateConnectionAsync(ct);
        await using var channel = await connection.CreateChannelAsync(cancellationToken: ct);

        await _options.DeclareTopologyAsync(channel, ct);
        await channel.BasicQosAsync(0, 1, false, ct);

        var closed = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        connection.ConnectionShutdownAsync += (_, _) => { closed.TrySetResult(); return Task.CompletedTask; };

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (_, ea) =>
        {
            try
            {
                await ProcessAsync(ea, ct);
                await channel.BasicAckAsync(ea.DeliveryTag, false, ct);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                await channel.BasicNackAsync(ea.DeliveryTag, false, true, CancellationToken.None);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Mensagem irrecuperável enviada à fila de erro. RoutingKey={RoutingKey}", ea.RoutingKey);
                await channel.BasicNackAsync(ea.DeliveryTag, false, false, CancellationToken.None);
            }
        };

        await channel.BasicConsumeAsync(_options.Queue, autoAck: false, consumer: consumer, cancellationToken: ct);
        _logger.LogInformation("Consumer RabbitMQ ativo na fila {Queue}.", _options.Queue);

        using var reg = ct.Register(() => closed.TrySetResult());
        await closed.Task;
    }

    private async Task ProcessAsync(BasicDeliverEventArgs ea, CancellationToken ct)
    {
        for (var attempt = 1; ; attempt++)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var handler = scope.ServiceProvider.GetRequiredService<IIntegrationEventHandler>();
                await handler.HandleAsync(ea.RoutingKey, ea.Body, ct);
                return;
            }
            catch (PermanentMessageException)
            {
                throw;
            }
            catch (Exception ex) when (attempt < MaxAttempts && !ct.IsCancellationRequested)
            {
                _logger.LogWarning(ex, "Falha transitória ({Attempt}/{Max}) em {RoutingKey}.", attempt, MaxAttempts, ea.RoutingKey);
                await Task.Delay(TimeSpan.FromSeconds(attempt * 2), ct);
            }
        }
    }
}
