namespace TechChallenge.OS.Infrastructure.Persistence;

/// <summary>Registro de eventos já processados (idempotência).</summary>
public class ProcessedEvent
{
    public Guid EventId { get; set; }
    public string RoutingKey { get; set; } = string.Empty;
    public Guid OrdemServicoUniqueCode { get; set; }
    public string Resultado { get; set; } = string.Empty;
    public DateTime ProcessadoEm { get; set; }
}

/// <summary>Compensação persistida (execution.falhou): bloqueia progressão da OS afetada.</summary>
public class OrdemServicoCompensacao
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OrdemServicoUniqueCode { get; set; }
    public Guid EventId { get; set; }
    public string Motivo { get; set; } = string.Empty;
    public string Status { get; set; } = "Pendente";
    public DateTime OccurredAt { get; set; }
    public DateTime CriadoEm { get; set; }
}
