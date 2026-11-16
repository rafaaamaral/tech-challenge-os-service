namespace TechChallenge.OS.Application.DTOs
{
    /// <summary>
    /// DTO para criar uma nova Ordem de Serviço.
    /// </summary>
    public class CreateOrdemServicoDto
    {
        /// <summary>
        /// ID do cliente proprietário do veículo.
        /// </summary>
        public required int ClienteId { get; set; }

        /// <summary>
        /// ID do veículo a ser servido.
        /// </summary>
        public required int VeiculoId { get; set; }

        /// <summary>
        /// Itens de serviço informados na solicitação inicial da OS.
        /// Mantido nesta etapa por dependência transitória do fluxo atual.
        /// </summary>
        public List<CreateOrdemServicoItemServicoDto> ItensServicos { get; set; } = new();

        /// <summary>
        /// Itens de peça/insumo informados na solicitação inicial da OS.
        /// Mantido nesta etapa por dependência transitória do fluxo atual.
        /// </summary>
        public List<CreateOrdemServicoItemPecaInsumoDto> ItensPecasInsumos { get; set; } = new();
    }
}
