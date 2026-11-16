namespace TechChallenge.OS.Application.DTOs
{
    /// <summary>
    /// DTO para criar um item de peça/insumo em uma Ordem de Serviço.
    /// </summary>
    public class CreateOrdemServicoItemPecaInsumoDto
    {
        /// <summary>
        /// ID da peça ou insumo a ser adicionado.
        /// </summary>
        public required int PecaInsumoId { get; set; }

        /// <summary>
        /// Descrição da peça/insumo (preenchida automaticamente).
        /// </summary>
        public string? Descricao { get; set; }

        /// <summary>
        /// Quantidade de unidades necessárias.
        /// </summary>
        public required int Quantidade { get; set; }

    }
}
