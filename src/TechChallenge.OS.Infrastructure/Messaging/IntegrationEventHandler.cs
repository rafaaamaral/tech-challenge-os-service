using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TechChallenge.OS.Domain.Common.Enums;
using TechChallenge.OS.Infrastructure.Persistence;

namespace TechChallenge.OS.Infrastructure.Messaging;

/// <summary>Falha irrecuperável: a mensagem não deve ser reentregue (vai para a fila de erro).</summary>
public sealed class PermanentMessageException : Exception
{
    public PermanentMessageException(string message, Exception? inner = null) : base(message, inner) { }
}

public interface IIntegrationEventHandler
{
    Task HandleAsync(string routingKey, ReadOnlyMemory<byte> body, CancellationToken cancellationToken);
}

/// <summary>
/// Aplica eventos de Billing/Execution à OS usando somente os métodos existentes do Domain.
/// Idempotência e estado da OS são gravados na mesma transação (SaveChanges único).
/// </summary>
public sealed class IntegrationEventHandler : IIntegrationEventHandler
{
    private readonly OSDbContext _db;
    private readonly ILogger<IntegrationEventHandler> _logger;

    public IntegrationEventHandler(OSDbContext db, ILogger<IntegrationEventHandler> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task HandleAsync(string routingKey, ReadOnlyMemory<byte> body, CancellationToken cancellationToken)
    {
        if (!RabbitMqOptions.ConsumedRoutingKeys.Contains(routingKey))
            throw new PermanentMessageException($"Routing key não suportada: {routingKey}");

        IntegrationEvent? evento;
        try
        {
            evento = JsonSerializer.Deserialize<IntegrationEvent>(body.Span, JsonSerializerOptions.Web);
        }
        catch (JsonException ex)
        {
            throw new PermanentMessageException("JSON inválido.", ex);
        }

        if (evento is null || evento.EventId == Guid.Empty || evento.OrdemServicoUniqueCode == Guid.Empty || evento.OccurredAt == default)
            throw new PermanentMessageException("Evento sem eventId, ordemServicoUniqueCode ou occurredAt válidos.");

        if (await _db.ProcessedEvents.AnyAsync(x => x.EventId == evento.EventId, cancellationToken))
        {
            _logger.LogInformation("Evento duplicado ignorado. EventId={EventId} RoutingKey={RoutingKey}", evento.EventId, routingKey);
            return;
        }

        var os = await _db.OrdensServico.FirstOrDefaultAsync(x => x.UniqueCode == evento.OrdemServicoUniqueCode && x.Ativo, cancellationToken)
            ?? throw new PermanentMessageException($"OS {evento.OrdemServicoUniqueCode} não encontrada.");

        var compensada = await _db.Compensacoes.AnyAsync(x => x.OrdemServicoUniqueCode == os.UniqueCode, cancellationToken);
        if (compensada && routingKey != RabbitMqOptions.RkExecutionFalhou)
            throw new PermanentMessageException($"OS {os.UniqueCode} está em compensação; evento {routingKey} não pode progredir a OS.");

        try
        {
            switch (routingKey)
            {
                case RabbitMqOptions.RkBillingAprovado:
                    // Menor ajuste: sem evento de "orçamento gerado", a OS recém-criada é levada a
                    // AguardandoAprovacao com a transição existente e, aprovado, segue para EmExecucao.
                    GarantirAguardandoAprovacao(os);
                    os.RegistrarOrcamentoAprovado();
                    os.RegistrarExecucaoIniciada();
                    break;
                case RabbitMqOptions.RkBillingReprovado:
                    GarantirAguardandoAprovacao(os);
                    os.RegistrarOrcamentoReprovado();
                    break;
                case RabbitMqOptions.RkExecutionFinalizada:
                    os.RegistrarExecucaoFinalizada();
                    break;
                case RabbitMqOptions.RkExecutionFalhou:
                    if (os.Status != StatusOrdemServico.EmExecucao)
                        throw new InvalidOperationException("Falha de execução só se aplica a OS em execução.");
                    if (!compensada)
                    {
                        _db.Compensacoes.Add(new OrdemServicoCompensacao
                        {
                            OrdemServicoUniqueCode = os.UniqueCode,
                            EventId = evento.EventId,
                            Motivo = Truncar(evento.Motivo) ?? "Falha na criação da execução",
                            OccurredAt = evento.OccurredAt,
                            CriadoEm = DateTime.UtcNow
                        });
                    }
                    break;
            }
        }
        catch (InvalidOperationException ex)
        {
            throw new PermanentMessageException($"Transição inválida para OS {os.UniqueCode} ({os.Status}) com {routingKey}: {ex.Message}", ex);
        }

        os.DataAlteracao = DateTime.UtcNow;
        _db.ProcessedEvents.Add(new ProcessedEvent
        {
            EventId = evento.EventId,
            RoutingKey = routingKey,
            OrdemServicoUniqueCode = os.UniqueCode,
            Resultado = os.Status.ToString(),
            ProcessadoEm = DateTime.UtcNow
        });

        await _db.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Evento {RoutingKey} aplicado. EventId={EventId} OS={OS} Status={Status}", routingKey, evento.EventId, os.UniqueCode, os.Status);
    }

    private static void GarantirAguardandoAprovacao(Domain.Aggregates.OrdemServicos.OrdemServico os)
    {
        if (os.Status is StatusOrdemServico.Recebida or StatusOrdemServico.EmDiagnostico)
            os.RegistrarAguardandoAprovacaoOrcamento();
    }

    private static string? Truncar(string? value) => value is { Length: > 500 } ? value[..500] : value;
}
