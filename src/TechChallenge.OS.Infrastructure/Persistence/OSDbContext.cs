using Microsoft.EntityFrameworkCore;
using TechChallenge.OS.Domain.Aggregates.OrdemServicos;
using TechChallenge.OS.Infrastructure.Persistence.Configurations;

namespace TechChallenge.OS.Infrastructure.Persistence;

public class OSDbContext : DbContext
{
    public const string OrdemServicoNumeroSequenceName = "os_numero_seq";

    public OSDbContext(DbContextOptions<OSDbContext> options) : base(options)
    {
    }

    public DbSet<OrdemServico> OrdensServico => Set<OrdemServico>();
    public DbSet<OrdemServicoItemServico> OrdemServicoItensServicos => Set<OrdemServicoItemServico>();
    public DbSet<OrdemServicoItemPecaInsumo> OrdemServicoItensPecasInsumos => Set<OrdemServicoItemPecaInsumo>();

    public DbSet<ProcessedEvent> ProcessedEvents => Set<ProcessedEvent>();
    public DbSet<OrdemServicoCompensacao> Compensacoes => Set<OrdemServicoCompensacao>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasSequence<int>(OrdemServicoNumeroSequenceName)
            .StartsAt(1)
            .IncrementsBy(1);

        modelBuilder.ApplyConfiguration(new OrdemServicoConfiguration());
        modelBuilder.ApplyConfiguration(new OrdemServicoItemServicoConfiguration());
        modelBuilder.ApplyConfiguration(new OrdemServicoItemPecaInsumoConfiguration());
        modelBuilder.ApplyConfiguration(new ProcessedEventConfiguration());
        modelBuilder.ApplyConfiguration(new OrdemServicoCompensacaoConfiguration());
    }
}
