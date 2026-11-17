using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;

namespace TechChallenge.OS.Infrastructure.Messaging;

public sealed class RabbitMqOptions
{
    public const string ExchangeName = "techchallenge.events";
    public const string ErrorExchangeName = "techchallenge.events.os.error";
    public const string ErrorRoutingKey = "os.error";

    public const string RkOsCriada = "os.criada";
    public const string RkBillingAprovado = "billing.aprovado";
    public const string RkBillingReprovado = "billing.reprovado";
    public const string RkExecutionFinalizada = "execution.finalizada";
    public const string RkExecutionFalhou = "execution.falhou";

    public static readonly string[] ConsumedRoutingKeys =
    {
        RkBillingAprovado, RkBillingReprovado, RkExecutionFinalizada, RkExecutionFalhou
    };

    public bool Enabled { get; init; } = true;
    public string Host { get; init; } = "localhost";
    public int Port { get; init; } = 5672;
    public string User { get; init; } = "guest";
    public string Password { get; init; } = "guest";
    public string VirtualHost { get; init; } = "/";
    public string Queue { get; init; } = "os-service.events";
    public string ErrorQueue => Queue + ".error";

    public static RabbitMqOptions FromConfiguration(IConfiguration configuration)
    {
        var defaults = new RabbitMqOptions();
        return new RabbitMqOptions
        {
            Enabled = !bool.TryParse(configuration["RABBITMQ_ENABLED"], out var enabled) || enabled,
            Host = configuration["RABBITMQ_HOST"] ?? defaults.Host,
            Port = int.TryParse(configuration["RABBITMQ_PORT"], out var port) ? port : defaults.Port,
            User = configuration["RABBITMQ_USER"] ?? defaults.User,
            Password = configuration["RABBITMQ_PASSWORD"] ?? defaults.Password,
            VirtualHost = configuration["RABBITMQ_VHOST"] ?? defaults.VirtualHost,
            Queue = configuration["RABBITMQ_QUEUE"] ?? defaults.Queue
        };
    }

    public ConnectionFactory CreateConnectionFactory() => new()
    {
        HostName = Host,
        Port = Port,
        UserName = User,
        Password = Password,
        VirtualHost = VirtualHost,
        ConsumerDispatchConcurrency = 1
    };

    public async Task DeclareTopologyAsync(IChannel channel, CancellationToken cancellationToken)
    {
        await channel.ExchangeDeclareAsync(ExchangeName, ExchangeType.Topic, durable: true, autoDelete: false, cancellationToken: cancellationToken);
        await channel.ExchangeDeclareAsync(ErrorExchangeName, ExchangeType.Direct, durable: true, autoDelete: false, cancellationToken: cancellationToken);

        await channel.QueueDeclareAsync(ErrorQueue, durable: true, exclusive: false, autoDelete: false, cancellationToken: cancellationToken);
        await channel.QueueBindAsync(ErrorQueue, ErrorExchangeName, ErrorRoutingKey, cancellationToken: cancellationToken);

        var arguments = new Dictionary<string, object?>
        {
            ["x-dead-letter-exchange"] = ErrorExchangeName,
            ["x-dead-letter-routing-key"] = ErrorRoutingKey
        };
        await channel.QueueDeclareAsync(Queue, durable: true, exclusive: false, autoDelete: false, arguments: arguments, cancellationToken: cancellationToken);
        foreach (var key in ConsumedRoutingKeys)
            await channel.QueueBindAsync(Queue, ExchangeName, key, cancellationToken: cancellationToken);
    }
}
