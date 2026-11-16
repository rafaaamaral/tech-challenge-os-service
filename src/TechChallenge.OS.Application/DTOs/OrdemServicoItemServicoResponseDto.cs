namespace TechChallenge.OS.Application.DTOs
{
    /// <summary>
    /// DTO para exibição de item de serviço.
    /// </summary>
    public class OrdemServicoItemServicoResponseDto
    {
        /// <summary>
        /// ID do item no banco de dados.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// ID do serviço referenciado.
        /// </summary>
        public int ServicoId { get; set; }

        /// <summary>
        /// Descrição do serviço.
        /// </summary>
        public string Descricao { get; set; } = string.Empty;

    }
}
