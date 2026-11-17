using TechChallenge.OS.Application.DTOs;

namespace TechChallenge.OS.Application.Interfaces.Services
{
    /// <summary>
    /// Interface para serviço de aplicação de Ordem de Serviço.
    /// Define os casos de uso disponíveis para gerenciar Ordens de Serviço.
    /// 
    /// Implementação: OrdemServicoApplicationService
    /// </summary>
    public interface IOrdemServicoApplicationService
    {
        /// <summary>
        /// Cria uma nova Ordem de Serviço.
        /// </summary>
        Task<OrdemServicoResponseDto> CriarAsync(CreateOrdemServicoDto dto);

        /// <summary>
        /// Lista todas as Ordens de Serviço.
        /// </summary>
        Task<List<OrdemServicoResponseDto>> ListarTodosAsync();

        /// <summary>
        /// Obtém uma Ordem de Serviço por ID.
        /// </summary>
        Task<OrdemServicoResponseDto> ObterPorIdAsync(int id);

        /// <summary>
        /// Consulta o status de uma Ordem de Serviço pelo UniqueCode.
        /// </summary>
        Task<OrdemServicoResponseDto> ConsultarStatusAsync(Guid uniqueCode);

        /// <summary>
        /// Entrega uma Ordem de Serviço (estado final).
        /// Transição: Finalizada → Entregue
        /// </summary>
        Task<OrdemServicoResponseDto> EntregarAsync(int id);

        /// <summary>
        /// Deleta uma Ordem de Serviço (exclusão lógica).
        /// </summary>
        Task DeletarAsync(int id);
    }
}
