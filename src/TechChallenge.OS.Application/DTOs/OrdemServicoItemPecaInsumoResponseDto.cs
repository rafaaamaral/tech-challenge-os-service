namespace TechChallenge.OS.Application.DTOs
{
    /// <summary>
    /// DTO para exibição de item de peça/insumo.
    /// </summary>
    public class OrdemServicoItemPecaInsumoResponseDto
    {
        /// <summary>
        /// ID do item no banco de dados.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// ID da peça/insumo referenciado.
        /// </summary>
        public int PecaInsumoId { get; set; }

        /// <summary>
        /// Descrição da peça/insumo.
        /// </summary>
        public string Descricao { get; set; } = string.Empty;

        /// <summary>
        /// Quantidade utilizada.
        /// </summary>
        public int Quantidade { get; set; }

    }
}
