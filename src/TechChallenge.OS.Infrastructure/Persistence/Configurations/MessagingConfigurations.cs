using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TechChallenge.OS.Infrastructure.Persistence.Configurations;

internal sealed class ProcessedEventConfiguration : IEntityTypeConfiguration<ProcessedEvent>
{
    public void Configure(EntityTypeBuilder<ProcessedEvent> builder)
    {
        builder.ToTable("processed_event");
        builder.HasKey(x => x.EventId);
        builder.Property(x => x.EventId).HasColumnName("event_id").ValueGeneratedNever();
        builder.Property(x => x.RoutingKey).HasColumnName("routing_key").HasMaxLength(100).IsRequired();
        builder.Property(x => x.OrdemServicoUniqueCode).HasColumnName("ordem_servico_unique_code").IsRequired();
        builder.Property(x => x.Resultado).HasColumnName("resultado").HasMaxLength(100).IsRequired();
        builder.Property(x => x.ProcessadoEm).HasColumnName("processado_em").IsRequired();
    }
}

internal sealed class OrdemServicoCompensacaoConfiguration : IEntityTypeConfiguration<OrdemServicoCompensacao>
{
    public void Configure(EntityTypeBuilder<OrdemServicoCompensacao> builder)
    {
        builder.ToTable("ordem_servico_compensacao");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(x => x.OrdemServicoUniqueCode).HasColumnName("ordem_servico_unique_code").IsRequired();
        builder.Property(x => x.EventId).HasColumnName("event_id").IsRequired();
        builder.Property(x => x.Motivo).HasColumnName("motivo").HasMaxLength(500).IsRequired();
        builder.Property(x => x.Status).HasColumnName("status").HasMaxLength(50).IsRequired();
        builder.Property(x => x.OccurredAt).HasColumnName("occurred_at").IsRequired();
        builder.Property(x => x.CriadoEm).HasColumnName("criado_em").IsRequired();
        builder.HasIndex(x => x.OrdemServicoUniqueCode).IsUnique();
    }
}
