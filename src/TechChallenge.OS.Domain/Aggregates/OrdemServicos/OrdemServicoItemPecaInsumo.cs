using TechChallenge.OS.Domain.Common.Entities;
using TechChallenge.OS.Domain.Exceptions;

namespace TechChallenge.OS.Domain.Aggregates.OrdemServicos
{
    /// <summary>
    /// Entity que representa um item de peça ou insumo adicionado a uma Ordem de Serviço.
    /// Pertence ao agregado OrdemServico.
    /// </summary>
    public class OrdemServicoItemPecaInsumo : Audit
    {
        /// <summary>
        /// Identificador da Ordem de Serviço to qual este item pertence.
        /// </summary>
        public int OrdemServicoId { get; set; }

        /// <summary>
        /// Identificador da Peça/Insumo no catálogo externo.
        /// Mantido como ID pois o microsserviço não é dono dessa entidade.
        /// </summary>
        public required int PecaInsumoId { get; set; }

        /// <summary>
        /// Descrição da peça/insumo (cópia do catálogo no momento da adição).
        /// </summary>
        public required string Descricao { get; set; } = string.Empty;

        /// <summary>
        /// Quantidade de unidades desta peça/insumo.
        /// </summary>
        public required int Quantidade { get; set; }

        /// <summary>
        /// Factory method para criar um novo item de peça/insumo de solicitação inicial.
        /// </summary>
        /// <param name="pecaInsumoId">ID da peça/insumo no catálogo externo</param>
        /// <param name="descricao">Descrição da peça/insumo informada na solicitação</param>
        /// <param name="quantidade">Quantidade solicitada</param>
        public static OrdemServicoItemPecaInsumo Criar(int pecaInsumoId, string descricao, int quantidade)
        {
            var item = new OrdemServicoItemPecaInsumo
            {
                UniqueCode = Guid.NewGuid(),
                PecaInsumoId = pecaInsumoId,
                Descricao = descricao,
                Quantidade = quantidade
            };

            item.Validar();
            return item;
        }

        /// <summary>
        /// Valida as regras de negócio do item de peça/insumo.
        /// </summary>
        /// <exception cref="DomainException">Lançado se alguma validação falhar</exception>
        private void Validar()
        {
            if (PecaInsumoId <= 0)
                throw new DomainException("A peça ou insumo é obrigatória.");
            if (string.IsNullOrWhiteSpace(Descricao))
                throw new DomainException("A descrição da peça ou insumo é obrigatória.");
            if (Quantidade <= 0)
                throw new DomainException("A quantidade da peça ou insumo deve ser maior que zero.");
        }
    }
}
