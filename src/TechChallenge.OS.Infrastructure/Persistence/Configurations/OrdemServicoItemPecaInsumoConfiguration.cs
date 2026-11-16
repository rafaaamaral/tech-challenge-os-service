using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechChallenge.OS.Domain.Aggregates.OrdemServicos;

namespace TechChallenge.OS.Infrastructure.Persistence.Configurations;

internal sealed class OrdemServicoItemPecaInsumoConfiguration : IEntityTypeConfiguration<OrdemServicoItemPecaInsumo>
{
    public void Configure(EntityTypeBuilder<OrdemServicoItemPecaInsumo> builder)
    {
        builder.ToTable("ordem_servico_item_peca_insumo");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.UniqueCode)
            .HasColumnName("unique_code")
            .IsRequired();

        builder.Property(x => x.OrdemServicoId)
            .HasColumnName("ordem_servico_id")
            .IsRequired();

        builder.Property(x => x.PecaInsumoId)
            .HasColumnName("peca_insumo_id")
            .IsRequired();

        builder.Property(x => x.Descricao)
            .HasColumnName("descricao")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.Quantidade)
            .HasColumnName("quantidade")
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
    }
}
