namespace TechChallenge.OS.Application.Interfaces.Services
{
    /// <summary>
    /// Interface para serviço de cliente (porta hexagonal).
    /// Representa a integração com o microsserviço/contexto de Cliente.
    /// 
    /// Implementação: Será preenchida quando o serviço de Cliente estiver disponível.
    /// Responsabilidade: Validar existência e propriedades do cliente (por ex.: ativo, crédito).
    /// </summary>
    public interface IClienteService
    {
        /// <summary>
        /// Valida se um cliente existe e está ativo.
        /// </summary>
        /// <param name="clienteId">ID do cliente a validar</param>
        /// <returns>True se cliente existe e está ativo, False caso contrário</returns>
        Task<bool> ClienteExisteEstaAtivoAsync(int clienteId);

        /// <summary>
        /// Obtém informações básicas do cliente para exibição.
        /// </summary>
        Task<(int Id, string Nome, string Email)?> ObterInfoClienteAsync(int clienteId);
    }
}
