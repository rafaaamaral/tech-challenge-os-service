using Xunit;

namespace TechChallenge.OS.UnitTests.Integration;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class PostgresAvailabilityFactAttribute : FactAttribute
{
    public PostgresAvailabilityFactAttribute()
    {
        var enabled = Environment.GetEnvironmentVariable("RUN_POSTGRES_INTEGRATION_TESTS");
        if (!string.Equals(enabled, "true", StringComparison.OrdinalIgnoreCase))
            Skip = "Teste de integração PostgreSQL desabilitado. Defina RUN_POSTGRES_INTEGRATION_TESTS=true para habilitar.";
    }
}
