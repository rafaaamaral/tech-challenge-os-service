using Microsoft.Extensions.Logging;
using TechChallenge.OS.Application.DTOs;
using TechChallenge.OS.Application.Interfaces.Services;

namespace TechChallenge.OS.Infrastructure.Messaging;

/// <summary>
/// Decorator que publica OrdemServicoCriada após a OS ser persistida, sem alterar a camada Application.
/// </summary>
public sealed class PublishingOrdemServicoApplicationService : IOrdemServicoApplicationService
{
    private readonly IOrdemServicoApplicationService _inner;
    private readonly IOrdemServicoEventPublisher _publisher;
    private readonly ILogger<PublishingOrdemServicoApplicationService> _logger;

    public PublishingOrdemServicoApplicationService(
        IOrdemServicoApplicationService inner,
        IOrdemServicoEventPublisher publisher,
        ILogger<PublishingOrdemServicoApplicationService> logger)
    {
        _inner = inner;
        _publisher = publisher;
        _logger = logger;
    }

    public async Task<OrdemServicoResponseDto> CriarAsync(CreateOrdemServicoDto dto)
    {
        var criada = await _inner.CriarAsync(dto);
        try
        {
            await _publisher.PublicarOrdemServicoCriadaAsync(criada.UniqueCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falha ao publicar OrdemServicoCriada para OS {OS}. A OS foi persistida; o evento NÃO foi entregue.", criada.UniqueCode);
        }
        return criada;
    }

    public Task<List<OrdemServicoResponseDto>> ListarTodosAsync() => _inner.ListarTodosAsync();
    public Task<OrdemServicoResponseDto> ObterPorIdAsync(int id) => _inner.ObterPorIdAsync(id);
    public Task<OrdemServicoResponseDto> ConsultarStatusAsync(Guid uniqueCode) => _inner.ConsultarStatusAsync(uniqueCode);
    public Task<OrdemServicoResponseDto> EntregarAsync(int id) => _inner.EntregarAsync(id);
    public Task DeletarAsync(int id) => _inner.DeletarAsync(id);
}
