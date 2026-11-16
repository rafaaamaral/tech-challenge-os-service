namespace TechChallenge.OS.Application.DTOs
{
    /// <summary>
    /// DTO para criar um item de serviço em uma Ordem de Serviço.
    /// </summary>
    public class CreateOrdemServicoItemServicoDto
    {
        /// <summary>
        /// ID do serviço a ser adicionado.
        /// </summary>
        public required int ServicoId { get; set; }

        /// <summary>
        /// Descrição do serviço (preenchida automaticamente com dados do serviço).
        /// </summary>
        public string? Descricao { get; set; }

    }
}
