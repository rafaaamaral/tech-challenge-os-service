using TechChallenge.OS.Domain.Common.Enums;

namespace TechChallenge.OS.Application.DTOs
{
    /// <summary>
    /// DTO de exibição da Ordem de Serviço no contexto do OS.
    /// </summary>
    public class OrdemServicoResponseDto
    {
        public int Id { get; set; }
        public Guid UniqueCode { get; set; }
        public int ClienteId { get; set; }
        public int VeiculoId { get; set; }
        public int Numero { get; set; }
        public StatusOrdemServico Status { get; set; }

        /// <summary>
        /// Snapshot da solicitação inicial de serviços.
        /// </summary>
        public List<OrdemServicoItemServicoResponseDto> ItensServicos { get; set; } = new();

        /// <summary>
        /// Snapshot da solicitação inicial de peças/insumos.
        /// </summary>
        public List<OrdemServicoItemPecaInsumoResponseDto> ItensPecasInsumos { get; set; } = new();
    }
}
