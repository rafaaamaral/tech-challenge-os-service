namespace TechChallenge.OS.Application.Interfaces.Services
{
    /// <summary>
    /// Interface para serviço de veículo (porta hexagonal).
    /// Representa a integração com o microsserviço/contexto de Veículo.
    /// 
    /// Implementação: Será preenchida quando o serviço de Veículo estiver disponível.
    /// Responsabilidade: Validar existência do veículo e sua propriedade (pertence ao cliente).
    /// </summary>
    public interface IVeiculoService
    {
        /// <summary>
        /// Valida se um veículo existe e pertence ao cliente especificado.
        /// </summary>
        /// <param name="veiculoId">ID do veículo a validar</param>
        /// <param name="clienteId">ID do cliente proprietário esperado</param>
        /// <returns>True se veículo existe e pertence ao cliente, False caso contrário</returns>
        Task<bool> VeiculoPertenceAoClienteAsync(int veiculoId, int clienteId);

        /// <summary>
        /// Obtém informações básicas do veículo para exibição.
        /// </summary>
        Task<(int Id, string Modelo, string Placa)?> ObterInfoVeiculoAsync(int veiculoId);
    }
}
