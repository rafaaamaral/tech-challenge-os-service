using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechChallenge.OS.Domain.Aggregates.OrdemServicos;

namespace TechChallenge.OS.Infrastructure.Persistence.Configurations;

internal sealed class OrdemServicoConfiguration : IEntityTypeConfiguration<OrdemServico>
{
    public void Configure(EntityTypeBuilder<OrdemServico> builder)
    {
        builder.ToTable("ordem_servico");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.UniqueCode)
            .HasColumnName("unique_code")
            .IsRequired();

        builder.Property(x => x.ClienteId)
            .HasColumnName("cliente_id")
            .IsRequired();

        builder.Property(x => x.VeiculoId)
            .HasColumnName("veiculo_id")
            .IsRequired();

        builder.Property(x => x.Numero)
            .HasColumnName("numero")
            .IsRequired();

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.Ativo)
            .HasColumnName("ativo")
            .IsRequired();

        builder.Property(x => x.CriadoPor)
            .HasColumnName("criado_por")
            .IsRequired();

        builder.Property(x => x.DataCriacao)
            .HasColumnName("data_criacao")
            .IsRequired();

        builder.Property(x => x.AlteradoPor)
            .HasColumnName("alterado_por");

        builder.Property(x => x.DataAlteracao)
            .HasColumnName("data_alteracao");

        builder.HasIndex(x => x.UniqueCode)
            .IsUnique();

        builder.HasIndex(x => x.Numero)
            .IsUnique();

        builder.HasMany(x => x.ItensServicos)
            .WithOne()
            .HasForeignKey(x => x.OrdemServicoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.ItensPecasInsumos)
            .WithOne()
            .HasForeignKey(x => x.OrdemServicoId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
