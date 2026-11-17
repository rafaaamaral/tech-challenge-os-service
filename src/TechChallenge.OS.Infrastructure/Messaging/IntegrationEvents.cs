namespace TechChallenge.OS.Infrastructure.Messaging;

/// <summary>
/// Contrato comum: eventId, ordemServicoUniqueCode e occurredAt (UTC). "motivo" é opcional.
/// </summary>
public record IntegrationEvent(Guid EventId, Guid OrdemServicoUniqueCode, DateTime OccurredAt, string? Motivo = null);

public sealed record OrdemServicoCriadaEvent(Guid EventId, Guid OrdemServicoUniqueCode, DateTime OccurredAt)
    : IntegrationEvent(EventId, OrdemServicoUniqueCode, OccurredAt);
