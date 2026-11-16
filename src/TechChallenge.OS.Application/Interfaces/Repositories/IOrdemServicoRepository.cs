using TechChallenge.OS.Domain.Aggregates.OrdemServicos;

namespace TechChallenge.OS.Application.Interfaces.Repositories
{
    /// <summary>
    /// Interface de repositório para Ordem de Serviço.
    /// Define as operações de persistência do agregado Ordem de Serviço.
    /// 
    /// Nota: Implementação será feita na camada Infrastructure.
    /// </summary>
    public interface IOrdemServicoRepository
    {
        /// <summary>
        /// Adiciona uma nova Ordem de Serviço ao repositório.
        /// </summary>
        Task<OrdemServico> AddAsync(OrdemServico ordemServico);

        /// <summary>
        /// Atualiza uma Ordem de Serviço existente.
        /// </summary>
        Task<OrdemServico> UpdateAsync(OrdemServico ordemServico);

        /// <summary>
        /// Obtém uma Ordem de Serviço por seu ID.
        /// </summary>
        Task<OrdemServico?> GetByIdAsync(int id);

        /// <summary>
        /// Obtém uma Ordem de Serviço por seu UniqueCode (Guid).
        /// </summary>
        Task<OrdemServico?> GetByUniqueCodeAsync(Guid uniqueCode);

        /// <summary>
        /// Obtém uma Ordem de Serviço com seus itens (serviços e peças) carregados.
        /// </summary>
        Task<OrdemServico?> GetByIdWithItemsAsync(int id);

        /// <summary>
        /// Lista todas as Ordens de Serviço com seus itens carregados.
        /// </summary>
        Task<IEnumerable<OrdemServico>> ListAllWithItemsAsync();

        /// <summary>
        /// Remove (exclusão lógica) uma Ordem de Serviço.
        /// </summary>
        Task DeleteAsync(int id);

        /// <summary>
        /// Obtém o próximo número sequencial de OS de forma concorrente-safe.
        /// </summary>
        Task<int> GetNextNumeroAsync();
    }
}
