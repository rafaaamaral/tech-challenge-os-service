namespace TechChallenge.OS.Application.Interfaces.Services
{
    /// <summary>
    /// Porta de saída para publicação de eventos de integração da OS.
    /// </summary>
    public interface IOrdemServicoEventPublisher
    {
        Task PublicarOrdemServicoCriadaAsync(Guid ordemServicoUniqueCode, CancellationToken cancellationToken = default);
    }
}
