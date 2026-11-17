using System.Diagnostics.CodeAnalysis;
using TechChallenge.OS.Application.DTOs;
using TechChallenge.OS.Application.Exceptions;

namespace TechChallenge.OS.Application.Validators
{
    /// <summary>
    /// Validador para DTOs de Ordem de Serviço.
    /// Centraliza as validações de entrada de dados da aplicação.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class OrdemServicoValidator
    {
        /// <summary>
        /// Valida DTO de criação de Ordem de Serviço.
        /// </summary>
        /// <exception cref="InvalidOperationException">Se alguma validação falhar</exception>
        public static void ValidarCreateOrdemServicoDto(CreateOrdemServicoDto dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            if (dto.ClienteId <= 0)
                throw new InvalidOperationException("ClienteId deve ser maior que zero.");

            if (dto.VeiculoId <= 0)
                throw new InvalidOperationException("VeiculoId deve ser maior que zero.");

            if (!dto.ItensServicos.Any() && !dto.ItensPecasInsumos.Any())
                throw new InvalidOperationException(
                    "A Ordem de Serviço deve possuir ao menos um serviço ou peça/insumo.");

            // Valida cada item de serviço
            foreach (var item in dto.ItensServicos)
            {
                if (item.ServicoId <= 0)
                    throw new InvalidOperationException(
                        "ServicoId de cada item deve ser maior que zero.");
            }

            // Valida cada item de peça/insumo
            foreach (var item in dto.ItensPecasInsumos)
            {
                if (item.PecaInsumoId <= 0)
                    throw new InvalidOperationException(
                        "PecaInsumoId de cada item deve ser maior que zero.");

                if (item.Quantidade <= 0)
                    throw new InvalidOperationException(
                        "Quantidade de cada peça/insumo deve ser maior que zero.");
            }
        }

        /// <summary>
        /// Valida DTO de item de serviço.
        /// </summary>
        public static void ValidarCreateOrdemServicoItemServicoDto(CreateOrdemServicoItemServicoDto dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            if (dto.ServicoId <= 0)
                throw new InvalidOperationException("ServicoId deve ser maior que zero.");
        }

        /// <summary>
        /// Valida DTO de item de peça/insumo.
        /// </summary>
        public static void ValidarCreateOrdemServicoItemPecaInsumoDto(CreateOrdemServicoItemPecaInsumoDto dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            if (dto.PecaInsumoId <= 0)
                throw new InvalidOperationException("PecaInsumoId deve ser maior que zero.");

            if (dto.Quantidade <= 0)
                throw new InvalidOperationException("Quantidade deve ser maior que zero.");
        }
    }
}
