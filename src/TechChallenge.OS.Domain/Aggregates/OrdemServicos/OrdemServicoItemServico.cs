using TechChallenge.OS.Domain.Common.Entities;
using TechChallenge.OS.Domain.Exceptions;

namespace TechChallenge.OS.Domain.Aggregates.OrdemServicos
{
    /// <summary>
    /// Entity que representa um item de serviço adicionado a uma Ordem de Serviço.
    /// Pertence ao agregado OrdemServico.
    /// </summary>
    public class OrdemServicoItemServico : Audit
    {
        /// <summary>
        /// Identificador da Ordem de Serviço to qual este item pertence.
        /// </summary>
        public int OrdemServicoId { get; set; }

        /// <summary>
        /// Identificador do Serviço no catálogo externo.
        /// Mantido como ID pois o microsserviço não é dono dessa entidade.
        /// </summary>
        public required int ServicoId { get; set; }

        /// <summary>
        /// Descrição do serviço (cópia do catálogo no momento da adição).
        /// </summary>
        public required string Descricao { get; set; } = string.Empty;

        /// <summary>
        /// Factory method para criar um novo item de serviço de solicitação inicial.
        /// </summary>
        /// <param name="servicoId">ID do serviço no catálogo externo</param>
        /// <param name="descricao">Descrição do serviço informado na solicitação</param>
        public static OrdemServicoItemServico Criar(int servicoId, string descricao)
        {
            var item = new OrdemServicoItemServico
            {
                UniqueCode = Guid.NewGuid(),
                ServicoId = servicoId,
                Descricao = descricao
            };

            item.Validar();
            return item;
        }

        /// <summary>
        /// Valida as regras de negócio do item de serviço.
        /// </summary>
        /// <exception cref="DomainException">Lançado se alguma validação falhar</exception>
        private void Validar()
        {
            if (ServicoId <= 0)
                throw new DomainException("O serviço é obrigatório.");
            if (string.IsNullOrWhiteSpace(Descricao))
                throw new DomainException("A descrição do serviço é obrigatória.");
        }
    }
}
