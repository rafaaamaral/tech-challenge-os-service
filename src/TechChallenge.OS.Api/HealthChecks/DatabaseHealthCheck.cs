using Microsoft.Extensions.Diagnostics.HealthChecks;
using TechChallenge.OS.Infrastructure.Persistence;

namespace TechChallenge.OS.Api.HealthChecks;

/// <summary>
/// Health check para validar conexão com PostgreSQL do TechChallenge.OS.
/// </summary>
public class DatabaseHealthCheck : IHealthCheck
{
    private readonly OSDbContext _dbContext;

    public DatabaseHealthCheck(OSDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Simples verificação de conectividade com uma query
            await _dbContext.Database.CanConnectAsync(cancellationToken);
            return HealthCheckResult.Healthy("PostgreSQL conectado e respondendo");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Falha na conexão com PostgreSQL", ex);
        }
    }
}
