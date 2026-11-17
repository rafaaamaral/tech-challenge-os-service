using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TechChallenge.OS.Infrastructure.Messaging;
using TechChallenge.OS.Application.Interfaces.Repositories;
using TechChallenge.OS.Application.Interfaces.Services;
using TechChallenge.OS.Application.Services.OrdemServicos;
using TechChallenge.OS.Infrastructure.Persistence;
using TechChallenge.OS.Infrastructure.Repositories;
using TechChallenge.OS.Infrastructure.Services;

namespace TechChallenge.OS.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("OSDb")
            ?? Environment.GetEnvironmentVariable("OSDB_CONNECTION_STRING");

        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("Connection string 'OSDb' não configurada.");

        services.AddDbContext<OSDbContext>(options =>
            options.UseNpgsql(connectionString));

        // Repositories
        services.AddScoped<IOrdemServicoRepository, OrdemServicoRepository>();

        // Application Services
        services.AddScoped<IOrdemServicoApplicationService>(sp =>
            new PublishingOrdemServicoApplicationService(
                ActivatorUtilities.CreateInstance<OrdemServicoApplicationService>(sp),
                sp.GetRequiredService<IOrdemServicoEventPublisher>(),
                sp.GetRequiredService<ILogger<PublishingOrdemServicoApplicationService>>()));

        var rabbit = RabbitMqOptions.FromConfiguration(configuration);
        services.AddSingleton(rabbit);
        if (rabbit.Enabled)
        {
            services.AddSingleton<IOrdemServicoEventPublisher, RabbitMqEventPublisher>();
            services.AddScoped<IIntegrationEventHandler, IntegrationEventHandler>();
            services.AddHostedService<RabbitMqConsumerService>();
        }
        else
        {
            services.AddSingleton<IOrdemServicoEventPublisher, NullOrdemServicoEventPublisher>();
        }

        // Hexagonal Ports (External Services) - Implementações de Stub
        // Essas interfaces serão substituídas pelas implementações reais quando os serviços correspondentes estiverem disponíveis
        services.AddScoped<IClienteService, ClienteServiceStub>();
        services.AddScoped<IVeiculoService, VeiculoServiceStub>();
        services.AddScoped<INotificacaoService, NotificacaoServiceStub>();

        return services;
    }
}
