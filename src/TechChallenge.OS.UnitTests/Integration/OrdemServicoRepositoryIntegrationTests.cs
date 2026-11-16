using Microsoft.EntityFrameworkCore;
using Npgsql;
using Xunit.Sdk;
using TechChallenge.OS.Domain.Aggregates.OrdemServicos;
using TechChallenge.OS.Domain.Common.Enums;
using TechChallenge.OS.Infrastructure.Persistence;
using TechChallenge.OS.Infrastructure.Repositories;

namespace TechChallenge.OS.UnitTests.Integration;

public sealed class OrdemServicoRepositoryIntegrationTests : IAsyncLifetime
{
    private string _connectionString = string.Empty;
    private bool _dbAvailable;

    public async Task InitializeAsync()
    {
        _connectionString = Environment.GetEnvironmentVariable("OSDB_INTEGRATION_TEST_CONNECTION")
            ?? "Host=localhost;Port=5432;Database=techchallenge_os_integration;Username=postgres;Password=postgres";

        try
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            _dbAvailable = true;

            await using var context = CreateContext();
            await context.Database.MigrateAsync();
        }
        catch
        {
            _dbAvailable = false;
        }
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [PostgresAvailabilityFact]
    public async Task AddAndGetByIdWithItemsAsync_DevePersistirOSComItens()
    {
        EnsureDatabaseAvailable();
        await using var context = CreateContext();
        var repository = new OrdemServicoRepository(context);

        var os = OrdemServico.Criar(1, 2, await repository.GetNextNumeroAsync());
        os.AdicionarServico(100, "Troca de óleo");
        os.AdicionarPecaInsumo(200, "Filtro", 2);

        var persisted = await repository.AddAsync(os);
        var loaded = await repository.GetByIdWithItemsAsync(persisted.Id);

        Assert.NotNull(loaded);
        Assert.Single(loaded!.ItensServicos);
        Assert.Single(loaded.ItensPecasInsumos);
    }

    [PostgresAvailabilityFact]
    public async Task GetByUniqueCodeAsync_DeveConsultarPorUniqueCode()
    {
        EnsureDatabaseAvailable();
        await using var context = CreateContext();
        var repository = new OrdemServicoRepository(context);

        var os = OrdemServico.Criar(10, 20, await repository.GetNextNumeroAsync());
        var persisted = await repository.AddAsync(os);

        var loaded = await repository.GetByUniqueCodeAsync(persisted.UniqueCode);

        Assert.NotNull(loaded);
        Assert.Equal(persisted.Id, loaded!.Id);
    }

    [PostgresAvailabilityFact]
    public async Task UpdateAsync_DevePersistirAlteracaoDeStatus()
    {
        EnsureDatabaseAvailable();
        await using var context = CreateContext();
        var repository = new OrdemServicoRepository(context);

        var os = OrdemServico.Criar(11, 22, await repository.GetNextNumeroAsync());
        os.RegistrarAguardandoAprovacaoOrcamento();
        os.RegistrarExecucaoIniciada();
        var persisted = await repository.AddAsync(os);

        persisted.RegistrarExecucaoFinalizada();
        persisted.Entregar();
        await repository.UpdateAsync(persisted);

        var loaded = await repository.GetByIdAsync(persisted.Id);

        Assert.NotNull(loaded);
        Assert.Equal(StatusOrdemServico.Entregue, loaded!.Status);
    }

    [PostgresAvailabilityFact]
    public async Task DeleteAsync_DeveAplicarExclusaoLogica()
    {
        EnsureDatabaseAvailable();
        await using var context = CreateContext();
        var repository = new OrdemServicoRepository(context);

        var os = OrdemServico.Criar(5, 6, await repository.GetNextNumeroAsync());
        var persisted = await repository.AddAsync(os);

        await repository.DeleteAsync(persisted.Id);

        var loadedById = await repository.GetByIdAsync(persisted.Id);
        var loadedByCode = await repository.GetByUniqueCodeAsync(persisted.UniqueCode);

        Assert.Null(loadedById);
        Assert.Null(loadedByCode);
    }

    [PostgresAvailabilityFact]
    public async Task AddAsync_DeveImporUnicidadeDeNumeroEUniqueCode()
    {
        EnsureDatabaseAvailable();
        await using var context = CreateContext();
        var repository = new OrdemServicoRepository(context);

        var numero = await repository.GetNextNumeroAsync();

        var os1 = OrdemServico.Criar(1, 1, numero);
        var os2 = OrdemServico.Criar(2, 2, numero);
        os2.UniqueCode = os1.UniqueCode;

        await repository.AddAsync(os1);

        await Assert.ThrowsAsync<DbUpdateException>(() => repository.AddAsync(os2));
    }

    [PostgresAvailabilityFact]
    public async Task GetNextNumeroAsync_DeveGerarSemDuplicidadeSobConcorrencia()
    {
        EnsureDatabaseAvailable();
        const int totalRequests = 30;

        await using var context = CreateContext();
        var repository = new OrdemServicoRepository(context);

        var tasks = Enumerable.Range(0, totalRequests)
            .Select(async _ =>
            {
                await using var ctx = CreateContext();
                var repo = new OrdemServicoRepository(ctx);
                return await repo.GetNextNumeroAsync();
            })
            .ToArray();

        var numeros = await Task.WhenAll(tasks);

        Assert.Equal(totalRequests, numeros.Distinct().Count());
    }

    private OSDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<OSDbContext>()
            .UseNpgsql(_connectionString)
            .Options;

        return new OSDbContext(options);
    }

    private void EnsureDatabaseAvailable()
    {
        if (!_dbAvailable)
            throw new XunitException("PostgreSQL indisponível para integração. Configure OSDB_INTEGRATION_TEST_CONNECTION com uma instância ativa.");
    }
}
