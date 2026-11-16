using Microsoft.EntityFrameworkCore;
using TechChallenge.OS.Application.Interfaces.Repositories;
using TechChallenge.OS.Domain.Aggregates.OrdemServicos;
using TechChallenge.OS.Infrastructure.Persistence;

namespace TechChallenge.OS.Infrastructure.Repositories;

public class OrdemServicoRepository : IOrdemServicoRepository
{
    private readonly OSDbContext _dbContext;

    public OrdemServicoRepository(OSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<OrdemServico> AddAsync(OrdemServico ordemServico)
    {
        ordemServico.Ativo = true;
        ordemServico.DataCriacao = DateTime.UtcNow;

        _dbContext.OrdensServico.Add(ordemServico);
        await _dbContext.SaveChangesAsync();
        return ordemServico;
    }

    public async Task<OrdemServico> UpdateAsync(OrdemServico ordemServico)
    {
        ordemServico.DataAlteracao = DateTime.UtcNow;

        _dbContext.OrdensServico.Update(ordemServico);
        await _dbContext.SaveChangesAsync();
        return ordemServico;
    }

    public async Task<OrdemServico?> GetByIdAsync(int id)
    {
        return await _dbContext.OrdensServico
            .FirstOrDefaultAsync(x => x.Id == id && x.Ativo);
    }

    public async Task<OrdemServico?> GetByUniqueCodeAsync(Guid uniqueCode)
    {
        return await _dbContext.OrdensServico
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UniqueCode == uniqueCode && x.Ativo);
    }

    public async Task<OrdemServico?> GetByIdWithItemsAsync(int id)
    {
        return await _dbContext.OrdensServico
            .Include(x => x.ItensServicos)
            .Include(x => x.ItensPecasInsumos)
            .FirstOrDefaultAsync(x => x.Id == id && x.Ativo);
    }

    public async Task<IEnumerable<OrdemServico>> ListAllWithItemsAsync()
    {
        return await _dbContext.OrdensServico
            .AsNoTracking()
            .Include(x => x.ItensServicos)
            .Include(x => x.ItensPecasInsumos)
            .Where(x => x.Ativo)
            .OrderBy(x => x.Numero)
            .ToListAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var ordemServico = await _dbContext.OrdensServico.FirstOrDefaultAsync(x => x.Id == id && x.Ativo);
        if (ordemServico is null)
            return;

        ordemServico.Ativo = false;
        ordemServico.DataAlteracao = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();
    }

    public async Task<int> GetNextNumeroAsync()
    {
        await using var command = _dbContext.Database.GetDbConnection().CreateCommand();
        command.CommandText = $"SELECT nextval('{OSDbContext.OrdemServicoNumeroSequenceName}')";

        if (command.Connection?.State != System.Data.ConnectionState.Open)
            await _dbContext.Database.OpenConnectionAsync();

        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }
}
