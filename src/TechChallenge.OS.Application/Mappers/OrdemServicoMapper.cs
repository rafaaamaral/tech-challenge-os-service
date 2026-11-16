using System.Diagnostics.CodeAnalysis;
using TechChallenge.OS.Application.DTOs;
using TechChallenge.OS.Domain.Aggregates.OrdemServicos;

namespace TechChallenge.OS.Application.Mappers
{
    /// <summary>
    /// Mapper para Ordem de Serviço.
    /// Converte entre Domain (OrdemServico) e DTOs (OrdemServicoResponseDto).
    /// Utiliza extension methods pelo padrão do projeto.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class OrdemServicoMapper
    {
        /// <summary>
        /// Converte uma entidade de domínio OrdemServico para seu DTO de resposta.
        /// </summary>
        public static OrdemServicoResponseDto ToResponseDto(this OrdemServico ordemServico)
        {
            if (ordemServico == null)
                throw new ArgumentNullException(nameof(ordemServico));

            var dto = new OrdemServicoResponseDto
            {
                Id = ordemServico.Id,
                UniqueCode = ordemServico.UniqueCode,
                ClienteId = ordemServico.ClienteId,
                VeiculoId = ordemServico.VeiculoId,
                Numero = ordemServico.Numero,
                Status = ordemServico.Status,
                ItensServicos = ordemServico.ItensServicos
                    .Select(x => new OrdemServicoItemServicoResponseDto
                    {
                        Id = x.Id,
                        ServicoId = x.ServicoId,
                        Descricao = x.Descricao
                    })
                    .ToList(),
                ItensPecasInsumos = ordemServico.ItensPecasInsumos
                    .Select(x => new OrdemServicoItemPecaInsumoResponseDto
                    {
                        Id = x.Id,
                        PecaInsumoId = x.PecaInsumoId,
                        Descricao = x.Descricao,
                        Quantidade = x.Quantidade
                    })
                    .ToList()
            };

            return dto;
        }

        /// <summary>
        /// Converte várias Order de Serviço para seus DTOs.
        /// </summary>
        public static List<OrdemServicoResponseDto> ToResponseDtoList(
            this IEnumerable<OrdemServico> ordensServico)
        {
            if (ordensServico == null)
                throw new ArgumentNullException(nameof(ordensServico));

            return ordensServico.Select(x => x.ToResponseDto()).ToList();
        }
    }
}
